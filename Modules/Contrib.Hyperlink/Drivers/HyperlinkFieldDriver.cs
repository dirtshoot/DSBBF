using System;
using JetBrains.Annotations;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Contrib.Hyperlink.ViewModels;
using Orchard.Localization;

namespace Contrib.Hyperlink.Drivers {
    [UsedImplicitly]
    public class HyperlinkFieldDriver : ContentFieldDriver<Fields.HyperlinkField> {
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/Contrib.Hyperlink"; // EditorTemplates/Fields/Contrib.Hyperlink.cshtml

        public HyperlinkFieldDriver(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        // Called when viewing the item with the custom field
        protected override DriverResult Display(ContentPart part, Fields.HyperlinkField field, string displayType, dynamic shapeHelper)
        {
            var viewModel = new HyperlinkFieldViewModel {
                Title = field.Title,
                DisplayText = field.DisplayText,
                Link = field.Link,
                OpenInNewTab = field.OpenInNewTab,
                Name = field.Name
            };

            return ContentShape("Fields_Contrib_Hyperlink", // this is just a key in the Shape Table
                                () => shapeHelper.Fields_Contrib_Hyperlink( // this is the actual Shape which will be resolved (Fields/Contrib.Hyperlink.cshtml)
                                            ContentField: field, Model: viewModel));
        }


        // Called when creating the field editor (GET)
        protected override DriverResult Editor(ContentPart part, Fields.HyperlinkField field, dynamic shapeHelper)
        {
            var viewModel = new HyperlinkFieldViewModel
            {
                Name = field.Name,
                Title = field.Title,
                DisplayText = field.DisplayText,
                Link = field.Link,
                OpenInNewTab = field.OpenInNewTab
            };
            
            return ContentShape("Fields_Contrib_Hyperlink_Edit", // this is just a key in the Shape Table
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: viewModel, Prefix: GetPrefix(field, part))); 
        }

        // Called when updating from the field editor (POST)
        protected override DriverResult Editor(ContentPart part, Fields.HyperlinkField field, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new HyperlinkFieldViewModel();

            if(updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null))
            {
                if (!string.IsNullOrWhiteSpace(viewModel.Link) && !Uri.IsWellFormedUriString(viewModel.Link, UriKind.RelativeOrAbsolute))
                {
                    updater.AddModelError(GetPrefix(field, part), T("{0} is an invalid hyperlink", field.Link));
                    field.Link = null;
                }
                else
                {
                    field.Title = viewModel.Title;
                    field.DisplayText = viewModel.DisplayText;
                    field.Link = viewModel.Link;
                    field.OpenInNewTab = viewModel.OpenInNewTab;
                }
            }
                
            return Editor(part, field, shapeHelper);
        }
    }
}
