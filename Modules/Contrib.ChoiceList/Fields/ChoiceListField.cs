using System;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;

namespace Contrib.ChoiceList.Fields {
    public class ChoiceListField : ContentField {

        public string Options
        {
            get { return Storage.Get<string>("Options"); }
            set { Storage.Set("Options", value ?? String.Empty); }
        }

        public string SelectedValue
        {
            get { return Storage.Get<string>("SelectedValue"); }
            set { Storage.Set("SelectedValue", value ?? String.Empty); }
        }
    }
}
