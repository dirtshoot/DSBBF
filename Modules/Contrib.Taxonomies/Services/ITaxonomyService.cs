using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;
using Contrib.Taxonomies.Models;
using System.Linq.Expressions;
using System;
using Orchard.ContentManagement.Records;
using Orchard.Data;

namespace Contrib.Taxonomies.Services {
    public interface ITaxonomyService : IDependency {
        IEnumerable<TaxonomyPart> GetTaxonomies();
        TaxonomyPart GetTaxonomy(int id);
        TaxonomyPart GetTaxonomyByName(string name);
        TaxonomyPart GetTaxonomyBySlug(string slug);
        void CreateTermContentType(TaxonomyPart taxonomy);
        void DeleteTaxonomy(TaxonomyPart taxonomy);
        void EditTaxonomy(TaxonomyPart taxonomy, string oldName);

        IEnumerable<TermPart> GetAllTerms();
        IEnumerable<TermPart> GetTerms(int taxonomyId);
        TermPart GetTerm(int id);
        TermPart GetTermByPath(string path);
        TermPart GetTermByName(int taxonomyId, string name);
        void DeleteTerm(TermPart termPart);
        void DeleteAssociatedTerms(ContentItem contentItem);
        void MoveTerm(TaxonomyPart taxonomy, TermPart term, TermPart parentTerm);
        void ProcessPath(TermPart term);
        IEnumerable<string> GetTermPaths();

        string GenerateTermTypeName(string taxonomyName);
        TermPart NewTerm(TaxonomyPart taxonomy);
        IEnumerable<TermPart> GetTermsForContentItem(int contentItemId, string field = null);
        void UpdateTerms(ContentItem contentItem, IEnumerable<TermPart> terms, string field);
        IEnumerable<TermPart> GetParents(TermPart term);
        IEnumerable<TermPart> GetChildren(TermPart term);
        IList<ContentItemRecord> GetContentItems(TermPart term, Expression<Func<TermContentItem, bool>> filter = null, Action<Orderable<TermContentItem>> order = null, int skip = 0, int count = 0);
        int GetContentItemsCount(TermPart term, Expression<Func<TermContentItem, bool>> filter = null);

        /// <summary>
        /// Returns all the slugs which can reach a taxonomy
        /// </summary>
        IEnumerable<string> GetSlugs();
    }
}