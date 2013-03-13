using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Contrib.Taxonomies.Services;
using Orchard.Localization;

namespace Contrib.Taxonomies.Settings {
    public class TaxonomyFieldEditorEvents : ContentDefinitionEditorEventsBase {
        private readonly ITaxonomyService _taxonomyService;

        public TaxonomyFieldEditorEvents(ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "TaxonomyField") {
                var model = definition.Settings.GetModel<TaxonomyFieldSettings>();
                model.Taxonomies = _taxonomyService.GetTaxonomies();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            var model = new TaxonomyFieldSettings();

            if ( updateModel.TryUpdateModel(model, "TaxonomyFieldSettings", null, null) ) {

                builder
                    .WithSetting("TaxonomyFieldSettings.TaxonomyId", model.TaxonomyId.ToString())
                    .WithSetting("TaxonomyFieldSettings.LeavesOnly", model.LeavesOnly.ToString())
                    .WithSetting("TaxonomyFieldSettings.SingleChoice", model.SingleChoice.ToString());
            }

            yield return DefinitionTemplate(model);
        }
    }
}