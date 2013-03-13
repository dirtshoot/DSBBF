using Contrib.YoutubeField.ViewModels;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Contrib.YoutubeField.Drivers {
    [UsedImplicitly]
    public class YoutubeFieldDriver : ContentFieldDriver<Fields.YoutubeField> {
        private static string GetPrefix(Fields.YoutubeField field, ContentPart part) {
            return string.Format("{0}.{1}", part.PartDefinition.Name, field.Name);
        }

        private static string GetDifferentiator(Fields.YoutubeField field) {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, Fields.YoutubeField field, string displayType, dynamic shapeHelper) {
            if (string.IsNullOrWhiteSpace(field.Identifier)) return null;

            return ContentShape("Fields_Contrib_Youtube",
                () => shapeHelper.Fields_Contrib_Youtube(ContentField: field));
        }

        protected override DriverResult Editor(ContentPart part, Fields.YoutubeField field, dynamic shapeHelper) {
            YoutubeFieldViewModel youtubeFieldViewModel = BuildViewModel(field);
            return ContentShape("Fields_Contrib_Youtube_Edit", GetDifferentiator(field),
                () => shapeHelper.EditorTemplate(TemplateName: "Fields.Contrib.Youtube.Edit", Model: youtubeFieldViewModel, Prefix: GetPrefix(field, part)));
        }

        protected override DriverResult Editor(ContentPart part, Fields.YoutubeField field, IUpdateModel updater, dynamic shapeHelper) {
            YoutubeFieldViewModel youtubeFieldViewModel = BuildViewModel(field);
            if (updater.TryUpdateModel(youtubeFieldViewModel, GetPrefix(field, part), null, null)) {
                youtubeFieldViewModel.UpdateField(field);
            }

            return Editor(part, field, shapeHelper);
        }

        protected YoutubeFieldViewModel BuildViewModel(Fields.YoutubeField youtubeField) {
            return new YoutubeFieldViewModel(youtubeField);
        }
    }
}