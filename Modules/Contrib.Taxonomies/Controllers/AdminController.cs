using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Routable.Services;
using Orchard.Localization;
using Orchard.Settings;
using Contrib.Taxonomies.Models;
using Contrib.Taxonomies.ViewModels;
using Contrib.Taxonomies.Services;
using Orchard.UI.Notify;

namespace Contrib.Taxonomies.Controllers {
    
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly ITaxonomyService _taxonomyService;
        private readonly IRoutableService _routableService;

        public AdminController(
            IOrchardServices services, 
            ITaxonomyService taxonomyService, 
            IRoutableService routableService ) {
            Services = services;
            _taxonomyService = taxonomyService;
            _routableService = routableService;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }
        
        public Localizer T { get; set; }

        public ActionResult Index() {
            IEnumerable<TaxonomyPart> taxonomies = _taxonomyService.GetTaxonomies();
            var entries = taxonomies.Select(CreateTaxonomyEntry).ToList();
            var model = new TaxonomyAdminIndexViewModel { Taxonomies = entries };
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(FormCollection input) {
            var viewModel = new TaxonomyAdminIndexViewModel { Taxonomies = new List<TaxonomyEntry>(), BulkAction = new TaxonomiesAdminIndexBulkAction() };
            
            if ( !TryUpdateModel(viewModel) ) {
                return View(viewModel);
            }

            var checkedEntries = viewModel.Taxonomies.Where(t => t.IsChecked);
            switch (viewModel.BulkAction) {
                case TaxonomiesAdminIndexBulkAction.None:
                    break;
                case TaxonomiesAdminIndexBulkAction.Delete:
                    if (!Services.Authorizer.Authorize(Permissions.ManageTaxonomies, T("Couldn't delete taxonomy")))
                        return new HttpUnauthorizedResult();

                    foreach (var entry in checkedEntries ) {
                        _taxonomyService.DeleteTaxonomy(entry.Taxonomy);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id) {
            if (!Services.Authorizer.Authorize(Permissions.CreateTaxonomy, T("Couldn't delete taxonomy")))
                return new HttpUnauthorizedResult();

            var taxonomy = _taxonomyService.GetTaxonomy(id);
            if (taxonomy == null) {
                return HttpNotFound();
            }

            _taxonomyService.DeleteTaxonomy(taxonomy);

            return RedirectToAction("Index");
        }

        public ActionResult Create() {
            if (!Services.Authorizer.Authorize(Permissions.CreateTaxonomy, T("Couldn't create taxonomy")))
                return new HttpUnauthorizedResult();

            var taxonomy = Services.ContentManager.New<TaxonomyPart>("Taxonomy");
            
            var model = Services.ContentManager.BuildEditor(taxonomy);
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost() {
            if (!Services.Authorizer.Authorize(Permissions.CreateTaxonomy, T("Couldn't create taxonomy")))
                return new HttpUnauthorizedResult();

            var taxonomy = Services.ContentManager.New<TaxonomyPart>("Taxonomy");
            var model = Services.ContentManager.UpdateEditor(taxonomy.ContentItem, this);

            if (_taxonomyService.GetTaxonomyByName(taxonomy.Name) != null) {
                ModelState.AddModelError("Title", T("A taxonomy with the same name already exists").ToString());
            }

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View((object)model);
            }

            Services.ContentManager.Create(taxonomy, VersionOptions.Published);
            _taxonomyService.CreateTermContentType(taxonomy);

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id) {
            if (!Services.Authorizer.Authorize(Permissions.CreateTaxonomy, T("Couldn't edit taxonomy")))
                return new HttpUnauthorizedResult();

            var taxonomy = _taxonomyService.GetTaxonomy(id);
            if(taxonomy == null) {
                return HttpNotFound();
            }

            var model = Services.ContentManager.BuildEditor(taxonomy);

            return View((object)model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id) {
            if (!Services.Authorizer.Authorize(Permissions.CreateTaxonomy, T("Couldn't edit taxonomy")))
                return new HttpUnauthorizedResult();

            var taxonomy = _taxonomyService.GetTaxonomy(id);
            var model = Services.ContentManager.UpdateEditor(taxonomy.ContentItem, this);

            // todo: validate taxonomy unique name
            //if (model.Name.Trim().ToLower() != taxonomy.Name.Trim().ToLower() && _taxonomyService.GetTaxonomyByName((string)model.Name) != null) {
            //    ModelState.AddModelError("Taxonomy", T("A taxonomy with the same name already exists").ToString());
            //}

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View((object)model);
            }

            _taxonomyService.EditTaxonomy(taxonomy, (string)model.Name);

            return RedirectToAction("Index");
        }

        public ActionResult Import(int id) {
            if (!Services.Authorizer.Authorize(Permissions.CreateTaxonomy, T("Couldn't import terms")))
                return new HttpUnauthorizedResult();

            var taxonomy = _taxonomyService.GetTaxonomy(id);

            if (taxonomy == null) {
                return HttpNotFound();
            }

            return View(taxonomy);
        }

        [HttpPost]
        public ActionResult Import(int id, string terms) {
            if (!Services.Authorizer.Authorize(Permissions.CreateTaxonomy, T("Couldn't import terms")))
                return new HttpUnauthorizedResult();

            var taxonomy = _taxonomyService.GetTaxonomy(id);

            if (taxonomy == null) {
                return HttpNotFound();
            }

            using(var reader = new StringReader(terms)) {
                string line;
                var previousLevel = 0;
                var parents = new Stack<TermPart>();
                var positions = new Stack<int>(); // todo: populate positions
                TermPart parentTerm = null;
                while (null != (line = reader.ReadLine())) {
                    // compute level from tabs
                    var level = 0;
                    while (line[level] == '\t') level++; // number of tabs to know the level

                    // create a new term content item
                    var term = _taxonomyService.NewTerm(taxonomy);

                    // detect parent term
                    if(level == previousLevel + 1) {
                        parentTerm = parents.Peek();
                        parents.Push(term);
                    }
                    else if(level == previousLevel) {
                        // same parent term
                        if (parents.Any())
                            parents.Pop();

                        parents.Push(term);
                    }
                    else if (level < previousLevel) {
                        for (var i = previousLevel; i >= level; i--)
                            parents.Pop();

                        parentTerm = parents.Any() ? parents.Peek() : null;
                        parents.Push(term);
                    }
                    
                    term.Container = parentTerm == null ? taxonomy : (IContent)parentTerm;

                    line = line.Trim();
                    int scIndex = line.IndexOf(';'); // seek first semi-colon to extract term and slug

                    // is there a semi-colon
                    if (scIndex != -1)
                    {
                        term.Name = line.Substring(0, scIndex);
                        term.Slug = line.Substring(scIndex + 1);
                    }
                    else {
                        term.Name = line;
                    }

                    if(_taxonomyService.GetTermByName(id, term.Name) != null) {
                        Services.Notifier.Information(T("A term with the same name already exist in this taxonomy: {0}", term.Name));
                        return View();
                    }

                    _routableService.ProcessSlug(term.As<IRoutableAspect>());

                    _taxonomyService.ProcessPath(term);
                    Services.ContentManager.Create(term, VersionOptions.Published);

                    previousLevel = level;
                }
            }

            Services.Notifier.Information(T("The terms have been imported successfully."));

            return RedirectToAction("Index", "TermAdmin", new { taxonomyId = id });
        }

        private static TaxonomyEntry CreateTaxonomyEntry(TaxonomyPart taxonomy) {
            return new TaxonomyEntry {
                Taxonomy = taxonomy,
                IsChecked = false,
            };
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
