using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Contrib.FileField.Settings {
    public class FileFieldEditorEvents : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "FileField") {
                var model = definition.Settings.GetModel<FileFieldSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            var model = new FileFieldSettings();
            if (updateModel.TryUpdateModel(model, "FileFieldSettings", null, null)) {
                builder.WithSetting("FileFieldSettings.Required", model.Required.ToString());
                builder.WithSetting("FileFieldSettings.OpenAction", model.OpenAction.ToString());
                builder.WithSetting("FileFieldSettings.Hint", model.Hint);
                builder.WithSetting("FileFieldSettings.MediaFolder", model.MediaFolder);
            }

            yield return DefinitionTemplate(model);
        }
    }
}