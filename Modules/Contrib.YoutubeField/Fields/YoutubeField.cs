using Orchard.ContentManagement;

namespace Contrib.YoutubeField.Fields {
    public class YoutubeField : ContentField {
        public string Identifier {
            get { return Storage.Get<string>("Identifier"); }
            set { Storage.Set("Identifier", value); }
        }

        public int Width {
            get { return Storage.Get<int>("Width"); }
            set { Storage.Set("Width", value); }
        }

        public int Height {
            get { return Storage.Get<int>("Height"); }
            set { Storage.Set("Height", value); }
        }
    }
}