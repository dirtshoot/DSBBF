using System;
using Contrib.ChoiceListField.Settings;
using JetBrains.Annotations;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Contrib.ChoiceList.ViewModels;
using Orchard.Localization;

namespace Contrib.ChoiceList.Drivers {
    [UsedImplicitly]
    public class ChoiceListFieldDriver : ContentFieldDriver<Fields.ChoiceListField> {
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/Contrib.ChoiceList"; // EditorTemplates/Fields/Contrib.ChoiceList.cshtml

        public ChoiceListFieldDriver(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        // Called when viewing the item with the custom field
        protected override DriverResult Display(ContentPart part, Fields.ChoiceListField field, string displayType, dynamic shapeHelper)
        {
            var viewModel = new ChoiceListFieldViewModel {
                Name = field.Name,
                SelectedValue = field.SelectedValue
            };

            return ContentShape("Fields_Contrib_ChoiceList", // this is just a key in the Shape Table
                                () => shapeHelper.Fields_Contrib_ChoiceList( // this is the actual Shape which will be resolved (Fields/Contrib.ChoiceList.cshtml)
                                            ContentField: field, Model: viewModel));
        }


        // Called when creating the field editor (GET)
        protected override DriverResult Editor(ContentPart part, Fields.ChoiceListField field, dynamic shapeHelper)
        {
            var settings = field.PartFieldDefinition.Settings.GetModel<ChoiceListFieldSettings>();

            var viewModel = new ChoiceListFieldViewModel {
                Name = field.Name,
                Options = settings.Options,
                SelectedValue = field.SelectedValue,
                ListMode = settings.ListMode
            };
            
            return ContentShape("Fields_Contrib_ChoiceList_Edit", // this is just a key in the Shape Table
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: viewModel, Prefix: GetPrefix(field, part))); 
        }

        // Called when updating from the field editor (POST)
        protected override DriverResult Editor(ContentPart part, Fields.ChoiceListField field, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new ChoiceListFieldViewModel();

            if(updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null)) {
                field.SelectedValue = viewModel.SelectedValue;
            }
                
            return Editor(part, field, shapeHelper);
        }
    }
}
