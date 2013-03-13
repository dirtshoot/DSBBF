using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Contrib.FileField.Settings;
using Contrib.FileField.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Media.Services;
using Orchard.Utility.Extensions;

namespace Contrib.FileField.Drivers {
    public class FileFieldDriver : ContentFieldDriver<Fields.FileField> {
        private const string TokenContentType = "{content-type}";
        private const string TokenFieldName = "{field-name}";
        private const string TokenContentItemId = "{content-item-id}";

        private readonly IMediaService _mediaService;
        public Localizer T { get; set; }

        public FileFieldDriver(IMediaService mediaService) {
            _mediaService = mediaService;
            T = NullLocalizer.Instance;
        }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private string GetDifferentiator(ContentField field, ContentPart part) {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, Fields.FileField field, string displayType, dynamic shapeHelper) {
            var settings = field.PartFieldDefinition.Settings.GetModel<FileFieldSettings>();
            return ContentShape("Fields_Contrib_File", GetDifferentiator(field, part), () => shapeHelper.Fields_Contrib_File(ContentPart: part, ContentField: field, Settings: settings));
        }

        protected override DriverResult Editor(ContentPart part, Fields.FileField field, dynamic shapeHelper) {
            var settings = field.PartFieldDefinition.Settings.GetModel<FileFieldSettings>();

            AssignDefaultMediaFolder(settings);

            var viewModel = new FileFieldViewModel {
                Settings = settings,
                Field = field
            };

            return ContentShape("Fields_Contrib_File_Edit", GetDifferentiator(field, part),
                                () => shapeHelper.EditorTemplate(TemplateName: "Fields/Contrib.File", Model: viewModel, Prefix: GetPrefix(field, part)));
        }

        protected override DriverResult Editor(ContentPart part, Fields.FileField field, IUpdateModel updater, dynamic shapeHelper) {
            var settings = field.PartFieldDefinition.Settings.GetModel<FileFieldSettings>();
            var viewModel = new FileFieldViewModel {
                Settings = settings,
                Field = field
            };

            if (updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null)) {
                var postedFile = ((Controller) updater).Request.Files["FileField-" + field.Name];

                AssignDefaultMediaFolder(settings);

                var mediaFolder = FormatWithTokens(settings.MediaFolder, part.ContentItem.ContentType, field.Name, part.ContentItem.Id);

                if (postedFile != null && postedFile.ContentLength != 0) {
                    if (_mediaService.FileAllowed(postedFile)) {
                        var postedFileLength = postedFile.ContentLength;
                        var postedFileStream = postedFile.InputStream;
                        var postedFileData = new byte[postedFileLength];
                        var postedFileName = Path.GetFileName(postedFile.FileName);
                        postedFileStream.Read(postedFileData, 0, postedFileLength);

                        string uniqueFileName = postedFileName;

                        try {
                            // try to create the folder before uploading a file into it
                            _mediaService.CreateFolder(null, mediaFolder);
                        }
                        catch {
                            // the folder can't be created because it already exists, continue
                        }

                        var existingFiles = _mediaService.GetMediaFiles(mediaFolder);
                        bool found = true;
                        var index = 0;
                        while (found) {
                            index++;
                            uniqueFileName = String.Format("{0}-{1}{2}", Path.GetFileNameWithoutExtension(postedFileName), index, Path.GetExtension(postedFileName));
                            string name = uniqueFileName;
                            found = existingFiles.Any(f => 0 == String.Compare(name, f.Name, StringComparison.OrdinalIgnoreCase));
                        }
                        field.Path = _mediaService.UploadMediaFile(mediaFolder, uniqueFileName, postedFileData, false);
                    }
                    else {
                        updater.AddModelError("File", T("The file type is not allowed for: {0}.", postedFile.FileName));
                    }
                }
                else {
                    if (settings.Required && string.IsNullOrWhiteSpace(field.Path)) {
                        updater.AddModelError("File", T("You must provide a file for {0}.", field.Name.CamelFriendly()));
                    }
                }

                if (string.IsNullOrWhiteSpace(field.Text)) {
                    field.Text = Path.GetFileName(field.Path);
                }
            }

            return Editor(part, field, shapeHelper);
        }

        private static string FormatWithTokens(string value, string contentType, string fieldName, int contentItemId) {
            if (String.IsNullOrWhiteSpace(value)) {
                return String.Empty;
            }

            return value
                .Replace(TokenContentType, contentType)
                .Replace(TokenFieldName, fieldName)
                .Replace(TokenContentItemId, Convert.ToString(contentItemId));
        }

        private static void AssignDefaultMediaFolder(FileFieldSettings settings) {
            if (String.IsNullOrWhiteSpace(settings.MediaFolder)) {
                settings.MediaFolder = TokenContentType + "/" + TokenFieldName;
            }
        }
    }
}