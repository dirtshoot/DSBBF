using System;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;

namespace Contrib.Hyperlink.Fields {
    public class HyperlinkField : ContentField {

        public string Title
        {
            get { return Storage.Get<string>("Title"); }
            set { Storage.Set("Title", value ?? String.Empty); }
        }

        public string DisplayText
        {
            get { return Storage.Get<string>("DisplayText"); }
            set { Storage.Set("DisplayText", value ?? String.Empty); }
        }

        public string Link
        {
            get { return Storage.Get<string>("Link"); }
            set { Storage.Set("Link", value ?? String.Empty); }
        }

        public bool OpenInNewTab {
            get { return Storage.Get<bool>("OpenInNewTab"); }
            set { Storage.Set("OpenInNewTab", value); }
        }
    }
}
