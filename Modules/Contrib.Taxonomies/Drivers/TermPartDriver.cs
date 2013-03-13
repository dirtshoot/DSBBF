using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Contrib.Taxonomies.Models;
using Contrib.Taxonomies.ViewModels;
using Orchard.ContentManagement.Aspects;
using System;
using Contrib.Taxonomies.Services;
using Orchard.Localization;

namespace Contrib.Taxonomies.Drivers {
    [UsedImplicitly]
    public class TermPartDriver : ContentPartDriver<TermPart> {

        private readonly ITaxonomyService _taxonomyService;

        public TermPartDriver(ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;
            T = NullLocalizer.Instance;
        }

        Localizer T { get; set; }

        protected override string Prefix { get { return "Term"; } }

        protected override DriverResult Editor(TermPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Taxonomies_Term_Fields",
                    () => shapeHelper.EditorTemplate(TemplateName: "Parts/Taxonomies.Term.Fields", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(TermPart termPart, IUpdateModel updater, dynamic shapeHelper) {
            if (updater.TryUpdateModel(termPart, Prefix, null, null)) {
                var existing = _taxonomyService.GetTermByName(termPart.TaxonomyId, termPart.Name);
                if (existing != null && existing != termPart) {
                    updater.AddModelError("Name", T("The term {0} already exists in this taxonomy", termPart.Name));
                }
            }

            return Editor(termPart, shapeHelper);
        }
    }
}