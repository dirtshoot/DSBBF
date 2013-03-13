using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Contrib.ChoiceListField.Settings
{
    public class ChoiceListFieldEditorEvents : ContentDefinitionEditorEventsBase
    {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "ChoiceListField")
            {
                var model = definition.Settings.GetModel<ChoiceListFieldSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            var model = new ChoiceListFieldSettings();
            if (updateModel.TryUpdateModel(model, "ChoiceListFieldSettings", null, null))
            {
                builder.WithSetting("ChoiceListFieldSettings.Options", model.Options);
                builder.WithSetting("ChoiceListFieldSettings.ListMode", model.ListMode);
            }

            yield return DefinitionTemplate(model);
        }
    }
}