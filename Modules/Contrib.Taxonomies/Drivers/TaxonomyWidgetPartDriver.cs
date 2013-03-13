using System.Web.Mvc;
using Contrib.Taxonomies.ViewModels;
using JetBrains.Annotations;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Contrib.Taxonomies.Models;
using Contrib.Taxonomies.Services;
using System.Collections.Generic;
using System;

namespace Contrib.Taxonomies.Drivers {
    [UsedImplicitly]
    public class TaxonomyWidgetPartDriver : ContentPartDriver<TaxonomyWidgetPart> {
        private readonly ITaxonomyService _taxonomyService;

        public TaxonomyWidgetPartDriver(ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;
        }

        protected override string Prefix { get { return "TaxonomyWidget"; } }

        protected override DriverResult Display(TaxonomyWidgetPart part, string displayType, dynamic shapeHelper) {
            var taxonomy = _taxonomyService.GetTaxonomy(part.TaxonomyPartRecord.Id);
            var terms = new List<TermPart>();
            if ( taxonomy != null ) {
                terms.AddRange(_taxonomyService.GetTerms(taxonomy.Id));
            }

            return ContentShape("Parts_Taxonomies_TermsWidget",
                () => shapeHelper.Parts_Taxonomies_TermsWidget(ContentPart: part, Terms: terms, Taxonomy: taxonomy));
        }

        protected override DriverResult Editor(TaxonomyWidgetPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(TaxonomyWidgetPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape(
                "Parts_Taxonomies_TermsWidget_Edit", () => {
                    var model = new TaxonomyWidgetViewModel();

                    if (updater != null) {
                        var oldSelectedTaxonomyId = model.SelectedTaxonomyId;
                        updater.TryUpdateModel(model, Prefix, null, null);
                        if (oldSelectedTaxonomyId!= model.SelectedTaxonomyId) {
                            part.TaxonomyPartRecord = _taxonomyService.GetTaxonomy(model.SelectedTaxonomyId).Record;
                        }
                    }

                    var taxonomies = _taxonomyService.GetTaxonomies();

                    var listItems = taxonomies.Select(taxonomy => new SelectListItem {
                        Value = Convert.ToString(taxonomy.Id),
                        Text = taxonomy.Name,
                        Selected = taxonomy.Record == part.TaxonomyPartRecord,
                    }).ToList();

                    model.AvailableTaxonomies = new SelectList(listItems, "Value", "Text", model.SelectedTaxonomyId);

                    return shapeHelper.EditorTemplate(TemplateName: "Parts/Taxonomies.TermsWidget", Model: model, Prefix: Prefix);
                });
        }
    }
}