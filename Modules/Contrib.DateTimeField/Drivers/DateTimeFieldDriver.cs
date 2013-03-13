using System;
using JetBrains.Annotations;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Contrib.DateTimeField.Settings;
using Contrib.DateTimeField.ViewModels;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Contrib.DateTimeField.Drivers {
    [UsedImplicitly]
    public class DateTimeFieldDriver : ContentFieldDriver<Fields.DateTimeField> {
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/Contrib.DateTime"; // EditorTemplates/Fields/Contrib.DateTime.cshtml

        public DateTimeFieldDriver(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        protected override DriverResult Display(ContentPart part, Fields.DateTimeField field, string displayType, dynamic shapeHelper) {
            var settings = field.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();
            var value = field.DateTime;

            var viewModel = new DateTimeFieldViewModel {
                Name = field.Name,
                Date = value.HasValue ? value.Value.ToLocalTime().ToShortDateString() : String.Empty,
                Time = value.HasValue ? value.Value.ToLocalTime().ToShortTimeString() : String.Empty,
                ShowDate = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.DateOnly,
                ShowTime = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.TimeOnly
            };

            return ContentShape("Fields_Contrib_DateTime", // this is just a key in the Shape Table
                () =>
                    shapeHelper.Fields_Contrib_DateTime( // this is the actual Shape which will be resolved (Fields/Contrib.DateTime.cshtml)
                        ContentField: field,
                        Model: viewModel )
                    );
        }

        protected override DriverResult Editor(ContentPart part, Fields.DateTimeField field, dynamic shapeHelper) {
           
            var settings = field.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();
            var value = field.DateTime;
            
            if(value.HasValue) {
                value = value.Value.ToLocalTime();
            }
            
            var viewModel = new DateTimeFieldViewModel {
                Name = field.Name,
                Date = value.HasValue ? value.Value.ToLocalTime().ToShortDateString() : "",
                Time = value.HasValue ? value.Value.ToLocalTime().ToShortTimeString() : "",
                ShowDate = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.DateOnly,
                ShowTime = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.TimeOnly

            };

            return ContentShape("Fields_Contrib_DateTime_Edit", // this is just a key in the Shape Table
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: viewModel, Prefix: GetPrefix(field, part))); 
        }

        protected override DriverResult Editor(ContentPart part, Fields.DateTimeField field, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new DateTimeFieldViewModel();

            if(updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null)) {
                DateTime value;

                var settings = field.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();
                if ( settings.Display == DateTimeFieldDisplays.DateOnly ) {
                    viewModel.Time = DateTime.Now.ToShortTimeString();
                }

                if ( settings.Display == DateTimeFieldDisplays.TimeOnly ) {
                    viewModel.Date = DateTime.Now.ToShortDateString();
                }

                if ( DateTime.TryParse(viewModel.Date + " " + viewModel.Time, out value) ) {
                    field.DateTime = value.ToUniversalTime();
                }
                else {
                    updater.AddModelError(GetPrefix(field, part), T("{0} is an invalid date and time", field.Name));
                    field.DateTime = null;
                }
            }
            
            return Editor(part, field, shapeHelper);
        }

        protected override void Importing(ContentPart part, Fields.DateTimeField field, ImportContentContext context) {
            var importedText = context.Attribute(GetPrefix(field, part), "DateTime");
            if (importedText != null) {
                field.Storage.Set(null, importedText);
            }
        }

        protected override void Exporting(ContentPart part, Fields.DateTimeField field, ExportContentContext context) {
            context.Element(GetPrefix(field, part)).SetAttributeValue("DateTime", field.Storage.Get<string>(null));
        }
    }
}
