using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;

namespace Contrib.MediaPickerField.Fields {
    public class MediaPickerField : ContentField {
        public string Url {
            get { return Storage.Get<string>(); }
            set { Storage.Set(value); }
        }
    }
}
