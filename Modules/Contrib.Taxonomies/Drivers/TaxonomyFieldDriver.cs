using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Contrib.Taxonomies.Fields;
using Contrib.Taxonomies.Services;
using Contrib.Taxonomies.Settings;
using Contrib.Taxonomies.ViewModels;
using Contrib.Taxonomies.Helpers;

namespace Contrib.Taxonomies.Drivers {
    [UsedImplicitly]
    public class TaxonomyFieldDriver : ContentFieldDriver<TaxonomyField> {
        private readonly ITaxonomyService _taxonomyService;
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/Contrib.TaxonomyField";

        public TaxonomyFieldDriver(IOrchardServices services, ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(TaxonomyField field, ContentPart part) {
            return field.Name;
        }
        protected override DriverResult Display(ContentPart part, TaxonomyField field, string displayType, dynamic shapeHelper) {
            var settings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>();
            var terms = _taxonomyService.GetTermsForContentItem(part.ContentItem.Id, field.Name).ToList();
            var taxonomy = _taxonomyService.GetTaxonomy(settings.TaxonomyId);

            return ContentShape("Fields_Contrib_TaxonomyField", GetDifferentiator(field, part),
                () =>
                    shapeHelper.Fields_Contrib_TaxonomyField(
                        ContentField: field,
                        Terms: terms,
                        Settings: settings,
                        Taxonomy: taxonomy)
                    );
        }

        protected override DriverResult Editor(ContentPart part, TaxonomyField field, dynamic shapeHelper) {
            var settings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>();

            var appliedTerms = _taxonomyService.GetTermsForContentItem(part.ContentItem.Id, field.Name);
            var terms = _taxonomyService.GetTerms(settings.TaxonomyId).Select(t => t.CreateTermEntry()).ToList();

            terms.ForEach(t => t.IsChecked = appliedTerms.Any(a => t.Id == a.Id));

            var viewModel = new TaxonomyFieldViewModel {
                Name = field.Name,
                Terms = terms,
                Settings = settings,
                SingleTermId = terms.Where(t => t.IsChecked).Select(t => t.Id).FirstOrDefault()
            };

            viewModel.Terms.ToList().ForEach(t => t.IsChecked = appliedTerms.Any(a => a.Id == t.Id ));

            return ContentShape("Fields_Contrib_TaxonomyField_Edit", GetDifferentiator(field, part),
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: viewModel, Prefix: GetPrefix(field, part)));
        }

        protected override DriverResult Editor(ContentPart part, TaxonomyField field, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new TaxonomyFieldViewModel { Terms =  new List<TermEntry>() };

            if(updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null)){
                _taxonomyService.UpdateTerms(part.ContentItem, viewModel.Terms.Where(t => t.IsChecked || t.Id == viewModel.SingleTermId).Select(t => _taxonomyService.GetTerm(t.Id)), field.Name);
            }

            return Editor(part, field, shapeHelper);
        }
    }
}
