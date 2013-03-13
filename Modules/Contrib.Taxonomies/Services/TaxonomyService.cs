using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Contrib.Taxonomies.Helpers;
using Contrib.Taxonomies.Models;
using JetBrains.Annotations;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.Core.Routable.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.UI.Notify;
using Contrib.Taxonomies.Routing;

namespace Contrib.Taxonomies.Services {

    [UsedImplicitly]
    public class TaxonomyService : ITaxonomyService {
        private readonly IRepository<TermContentItem> _termContentItemRepository;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardServices _services;
        private readonly ITaxonomySlugConstraint _slugContraint;

        public TaxonomyService(
            IRepository<TermContentItem> termContentItemRepository,
            IContentManager contentManager,
            INotifier notifier,
            IContentDefinitionManager contentDefinitionManager,
            IAuthorizationService authorizationService,
            IOrchardServices services,
            ITaxonomySlugConstraint slugConstraint) {
            _termContentItemRepository = termContentItemRepository;
            _contentManager = contentManager;
            _notifier = notifier;
            _authorizationService = authorizationService;
            _contentDefinitionManager = contentDefinitionManager;
            _services = services;
            _slugContraint = slugConstraint;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public IEnumerable<TaxonomyPart> GetTaxonomies() {
            return _contentManager.Query<TaxonomyPart>().List();
        }

        public TaxonomyPart GetTaxonomy(int id) {
            return _contentManager.Get(id).As<TaxonomyPart>();
        }

        public TaxonomyPart GetTaxonomyByName(string name) {
            if (String.IsNullOrWhiteSpace(name)) {
                throw new ArgumentNullException("name");
            }

            return _contentManager
                .Query<TaxonomyPart>()
                .Join<RoutePartRecord>()
                .Where(r => r.Title == name)
                .List()
                .FirstOrDefault();
        }

        public TaxonomyPart GetTaxonomyBySlug(string slug) {
            if (String.IsNullOrWhiteSpace(slug)) {
                throw new ArgumentNullException("slug");
            }

            return _contentManager
                .Query<TaxonomyPart, TaxonomyPartRecord>()
                .Join<RoutePartRecord>()
                .Where(r => r.Slug == slug)
                .List()
                .FirstOrDefault();
        }

        public void CreateTermContentType(TaxonomyPart taxonomy) {
            // create the associated term's content type
            taxonomy.TermTypeName = GenerateTermTypeName(taxonomy.Name);

            _contentDefinitionManager.AlterTypeDefinition(taxonomy.TermTypeName, 
                cfg => cfg
                    .WithSetting("TaxonomyId", taxonomy.Id.ToString())
                    .WithPart("TermPart")
                    .WithPart("RoutePart")
                    .WithPart("CommonPart")
                    .DisplayedAs(taxonomy.Name + " Term")
                );

            RefreshSlugConstraints();
        }

        public void EditTaxonomy(TaxonomyPart taxonomy, string oldName) {            
            // rename term definition
            _contentDefinitionManager.AlterTypeDefinition(taxonomy.TermTypeName,
                cfg => cfg.DisplayedAs(taxonomy.Name + " Term"));

            RefreshSlugConstraints();
        }

        public void DeleteTaxonomy(TaxonomyPart taxonomy) {
            _contentManager.Remove(taxonomy.ContentItem);

            RefreshSlugConstraints();

            // removing terms
            foreach (var term in GetTerms(taxonomy.Id)) {
                DeleteTerm(term);
            }

            // todo: delete dynamic content type
        }

        public string GenerateTermTypeName(string taxonomyName) {
            var disallowed = new Regex(@"[^\w]+");
            return disallowed.Replace(taxonomyName, "-");
        }

        public TermPart NewTerm(TaxonomyPart taxonomy) {
            var term = _contentManager.New<TermPart>(taxonomy.TermTypeName);
            term.TaxonomyId = taxonomy.Id;

            return term;
        }

        public IEnumerable<TermPart> GetTerms(int taxonomyId) {
            return _contentManager.Query<TermPart, TermPartRecord>().Where(x => x.TaxonomyId == taxonomyId).List().OrderBy(t => t);
        }

        public TermPart GetTermByPath(string path) {
            return _contentManager.Query<TermPart, TermPartRecord>()
                .Join<RoutePartRecord>()
                .Where(rr => rr.Path == path)
                .List()
                .FirstOrDefault();
        }

        public IEnumerable<TermPart> GetAllTerms() {
            return _contentManager.Query<TermPart, TermPartRecord>().List().OrderBy(t => t);
        }

        public TermPart GetTerm(int id) {
            return _contentManager.Query<TermPart, TermPartRecord>().Where(x => x.Id == id).List().FirstOrDefault();
        }

        public IEnumerable<TermPart> GetTermsForContentItem(int contentItemId, string field = null) {
            return String.IsNullOrEmpty(field) 
                ? _termContentItemRepository.Fetch(x => x.ContentItemRecord.Id == contentItemId).Select(t => GetTerm(t.TermRecord.Id)).OrderBy(t => t)
                : _termContentItemRepository.Fetch(x => x.ContentItemRecord.Id == contentItemId && x.Field == field).Select(t => GetTerm(t.TermRecord.Id)).OrderBy(t => t);
        }

        public TermPart GetTermByName(int taxonomyId, string name) {
            return _contentManager
                .Query<TermPart, TermPartRecord>()
                .Where(t => t.TaxonomyId == taxonomyId)
                .Join<RoutePartRecord>()
                .Where(r => r.Title == name)
                .List()
                .FirstOrDefault();
        }

        public void CreateTerm(TermPart termPart) {
            if (GetTermByName(termPart.TaxonomyId, termPart.Name) == null) {
                _authorizationService.CheckAccess(Permissions.CreateTerm, _services.WorkContext.CurrentUser, null);

                termPart.As<ICommonPart>().Container = GetTaxonomy(termPart.TaxonomyId).ContentItem;

                _contentManager.Create(termPart);
            }
            else {
                _notifier.Warning(T("The term {0} already exists in this taxonomy", termPart.Name));
            }
        }

        public void DeleteTerm(TermPart termPart) {
            _contentManager.Remove(termPart.ContentItem);

            foreach(var childTerm in GetChildren(termPart)) {
                _contentManager.Remove(childTerm.ContentItem);
            }

            // delete ternContentItems
            var termContentItems = _termContentItemRepository
                .Fetch(t => t.TermRecord == termPart.Record)
                .ToList();

            foreach (var termContentItem in termContentItems) {
                _termContentItemRepository.Delete(termContentItem);
            }
        }

        public void DeleteAssociatedTerms(ContentItem contentItem) {
            var termContentItems = _termContentItemRepository
                .Fetch(t => t.ContentItemRecord == contentItem.Record)
                .ToList();
            
            foreach(var termContentItem in termContentItems) {
                _termContentItemRepository.Delete(termContentItem);
            }
        }

        public void UpdateTerms(ContentItem contentItem, IEnumerable<TermPart> terms, string field) {
            var currentTerms = GetTermsForContentItem(contentItem.Id, field);

            // search terms to add
            var newTerms = terms.Where(term => !currentTerms.Any(t => t.Id == term.Id)).ToList();

            // search terms to remove
            var oldTerms = currentTerms.Where(term => !terms.Any(t => t.Id == term.Id)).ToList();

            // add new terms
            foreach(var term in newTerms) {
                _termContentItemRepository.Create(new TermContentItem {ContentItemRecord = contentItem.Record, TermRecord = term.Record, Field = field});
            }

            // remove old terms
            foreach ( var term in oldTerms ) {
                var termRecord = term.Record;
                var termContentItem = _termContentItemRepository.Get(tc => tc.ContentItemRecord == contentItem.Record && tc.TermRecord == termRecord && tc.Field == field);
                if ( termContentItem != null ) {
                    _termContentItemRepository.Delete(termContentItem);
                }
            }

            // adds all terms to the default storage slot to enable full text search
            // the field indexing logic tries to get a value from Storage.Get(null) by default

            var termNames = String.Join(", ", currentTerms.Union(newTerms).Except(oldTerms).Select(term => term.Name).ToArray());
            contentItem.Parts.SelectMany(part => part.Fields) // browse all fields
                .Where(f => f.Name == field)
                .Single()
                .Storage.Set(null, termNames);
        }

        public int GetContentItemsCount(TermPart term, 
            Expression<Func<TermContentItem, bool>> filter = null) {
            
            var predicate = GetContentItemsPredicate(term, filter);

            return _termContentItemRepository.Count(predicate);
        }

        public IList<ContentItemRecord> GetContentItems(TermPart term, 
            Expression<Func<TermContentItem, bool>> filter = null,
            Action<Orderable<TermContentItem>> order = null, 
            int skip = 0,
            int count = 0) {

            var predicate = GetContentItemsPredicate(term, filter);

            IEnumerable<TermContentItem> termContentItems;
            if(order != null) {
                if(skip > 0 || count > 0) {
                    termContentItems = _termContentItemRepository.Fetch(predicate, order);
                } 
                else {
                    termContentItems = _termContentItemRepository.Fetch(predicate, order, skip, count);
                }
            } 
            else {
                termContentItems = _termContentItemRepository.Fetch(predicate);
            }

            return termContentItems.Select(x => x.ContentItemRecord).Distinct().ToList();
        }

        private Expression<Func<TermContentItem, bool>> GetContentItemsPredicate(TermPart term, Expression<Func<TermContentItem, bool>> filter = null) {
            var predicate = PredicateBuilder.False<TermContentItem>();
            predicate = predicate.Or(t => t.TermRecord == term.Record);

            predicate = GetChildren(term)
                .Select(child => child.Id)
                .Aggregate(predicate, (current, id) => current.Or(t => t.TermRecord.Id == id));

            if (filter != null) {
                predicate = predicate.And(filter);
            }

            return predicate;
        }

        public IEnumerable<TermPart> GetChildren(TermPart term){
            return _contentManager.Query<TermPart, TermPartRecord>()
                .Where(x => (x.Path).StartsWith(term.FullPath))
                .List()
                .OrderBy(t => t)
                .Where(x => (x.FullPath + "/").StartsWith(term.FullPath + "/")); // add a trailing slash to prevent /123 to be recognized under /12
        }

        public IEnumerable<TermPart> GetParents(TermPart term) {
            return term.Path.Split('/').Select(id => GetTerm(int.Parse(id)));
        }

        public IEnumerable<string> GetSlugs() {
            return _contentManager
                .Query<TaxonomyPart>()
                .Join<RoutePartRecord>()
                .List()
                .Select(t => t.Slug);
        }

        public IEnumerable<string> GetTermPaths() {
            return _contentManager
                .Query<TermPart>()
                .Join<RoutePartRecord>()
                .List()
                .Select(t => t.As<IRoutableAspect>().Path);
        }

        public void MoveTerm(TaxonomyPart taxonomy, TermPart term, TermPart parentTerm) {
            term.Container = parentTerm == null ? taxonomy.ContentItem : parentTerm.ContentItem;
            ProcessPath(term);

            foreach (var childTerm in GetChildren(term)) {
                ProcessPath(childTerm);
            }
        }

        public void ProcessPath(TermPart term) {
            var parentTerm = term.Container.As<TermPart>();
            term.Path = parentTerm != null ? parentTerm.FullPath : String.Empty;
        }

        public void RefreshSlugConstraints() {
            _slugContraint.SetSlugs(GetSlugs());
        }
    }
}
