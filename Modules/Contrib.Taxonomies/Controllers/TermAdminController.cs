﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.Localization;
using Contrib.Taxonomies.ViewModels;
using Contrib.Taxonomies.Services;
using Orchard.UI.Admin;
using Orchard.ContentManagement;
using Orchard.UI.Notify;
using Contrib.Taxonomies.Helpers;

namespace Contrib.Taxonomies.Controllers {
    [ValidateInput(false), Admin]
    public class TermAdminController : Controller, IUpdateModel {
        private readonly ITaxonomyService _taxonomyService;

        public TermAdminController(IOrchardServices services, ITaxonomyService taxonomyService) {
            Services = services;
            _taxonomyService = taxonomyService;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }

        public Localizer T { get; set; }

        public ActionResult Index(int taxonomyId) {
            var taxonomy = _taxonomyService.GetTaxonomy(taxonomyId);
            var terms = _taxonomyService.GetTerms(taxonomyId);
            var entries = terms.Select(term => term.CreateTermEntry()).ToList();
            var model = new TermAdminIndexViewModel { Terms = entries, Taxonomy = taxonomy, TaxonomyId = taxonomyId };
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(FormCollection input) {
            var viewModel = new TermAdminIndexViewModel { Terms = new List<TermEntry>(), BulkAction = new TermsAdminIndexBulkAction() };

            if (!TryUpdateModel(viewModel)) {
                return View(viewModel);
            }

            IEnumerable<TermEntry> checkedEntries = viewModel.Terms.Where(t => t.IsChecked);
            switch (viewModel.BulkAction) {
                case TermsAdminIndexBulkAction.None:
                    break;
                case TermsAdminIndexBulkAction.Delete:
                    if (!Services.Authorizer.Authorize(Permissions.ManageTerms, T("Couldn't delete term")))
                        return new HttpUnauthorizedResult();

                    foreach (var entry in checkedEntries) {
                        var term = _taxonomyService.GetTerm(entry.Id);
                        _taxonomyService.DeleteTerm(term);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            Services.Notifier.Information(T("{0} term have been removed.", checkedEntries.Count()));

            return RedirectToAction("Index", new { taxonomyId = viewModel.TaxonomyId });
        }

        public ActionResult SelectTerm(int taxonomyId) {
            if (!Services.Authorizer.Authorize(Permissions.CreateTerm, T("Not allowed to create terms")))
                return new HttpUnauthorizedResult();

            var terms = _taxonomyService.GetTerms(taxonomyId);

            if (terms.Any()) {
                var model = new SelectTermViewModel {
                    Terms = _taxonomyService.GetTerms(taxonomyId),
                    SelectedTermId = -1
                };

                return View(model);
            }

            return RedirectToAction("Create", new { taxonomyId, parentTermId = -1 });
        }

        [HttpPost]
        public ActionResult SelectTerm(int taxonomyId, int selectedTermId) {
            if (!Services.Authorizer.Authorize(Permissions.ManageTerms, T("Not allowed to select terms")))
                return new HttpUnauthorizedResult();

            return RedirectToAction("Create", new { taxonomyId, parentTermId = selectedTermId });
        }

        public ActionResult MoveTerm(int taxonomyId, int termId) {
            if (!Services.Authorizer.Authorize(Permissions.CreateTerm, T("Not allowed to create terms")))
                return new HttpUnauthorizedResult();
            
            var term = _taxonomyService.GetTerm(termId);
            if(term == null) {
                return HttpNotFound();
            }

            var model = new MoveTermViewModel {
                Terms = _taxonomyService.GetTerms(taxonomyId).Where(t => !(t.FullPath + "/").StartsWith(term.FullPath + "/")),
                TermId = termId,
                SelectedTermId = -1
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult MoveTerm(int taxonomyId, int selectedTermId, int termId) {
            if (!Services.Authorizer.Authorize(Permissions.ManageTerms, T("Not allowed to select terms")))
                return new HttpUnauthorizedResult();

            var taxonomy = _taxonomyService.GetTaxonomy(taxonomyId);
            var parentTerm = _taxonomyService.GetTerm(selectedTermId);
            var term = _taxonomyService.GetTerm(termId);

            _taxonomyService.MoveTerm(taxonomy, term, parentTerm);

            return RedirectToAction("Index", new { taxonomyId });
        }

        public ActionResult Create(int taxonomyId, int parentTermId) {
            if (!Services.Authorizer.Authorize(Permissions.CreateTerm, T("Not allowed to create terms")))
                return new HttpUnauthorizedResult();

            var taxonomy = _taxonomyService.GetTaxonomy(taxonomyId);
            var parentTerm = _taxonomyService.GetTerm(parentTermId);
            var term = _taxonomyService.NewTerm(taxonomy);
            
            // assign a container to show the full route while editing
            term.Container = parentTerm == null ? taxonomy : (IContent)parentTerm;

            var model = Services.ContentManager.BuildEditor(term);
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(int taxonomyId, int parentTermId) {
            if (!Services.Authorizer.Authorize(Permissions.CreateTerm, T("Couldn't create term")))
                return new HttpUnauthorizedResult();

            var taxonomy = _taxonomyService.GetTaxonomy(taxonomyId);
            var parentTerm = _taxonomyService.GetTerm(parentTermId);

            var term = _taxonomyService.NewTerm(taxonomy);
            term.Container = parentTerm == null ? taxonomy.ContentItem : parentTerm.ContentItem;

            var model = Services.ContentManager.UpdateEditor(term, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View((object)model);
            }

            _taxonomyService.ProcessPath(term);

            Services.ContentManager.Create(term, VersionOptions.Published);

            Services.Notifier.Information(T("The {0} term has been created.", term.Name));

            return RedirectToAction("Index", "TermAdmin", new { taxonomyId });
        }

        public ActionResult Edit(int id) {

            if (!Services.Authorizer.Authorize(Permissions.ManageTerms, T("Not allowed to manage taxonomies")))
                return new HttpUnauthorizedResult();

            var term = _taxonomyService.GetTerm(id);
            if (term == null)
                return HttpNotFound();

            var model = Services.ContentManager.BuildEditor(term);
            return View((object)model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageTaxonomies, T("Couldn't edit taxonomy")))
                return new HttpUnauthorizedResult();

            var term = _taxonomyService.GetTerm(id);
            if (term == null)
                return HttpNotFound();

            var taxonomy = _taxonomyService.GetTaxonomy(term.TaxonomyId);

            var model = Services.ContentManager.UpdateEditor(term, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View((object)model);
            }

            _taxonomyService.ProcessPath(term);

            Services.Notifier.Information(T("Term information updated"));

            return RedirectToAction("Index", "TermAdmin", new { taxonomyId = taxonomy.Id });
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
