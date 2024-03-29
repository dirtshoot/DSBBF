using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace FeaturedItemSlider.Models {
    public class FeaturedItemPart : ContentPart<FeaturedItemPartRecord> {
        
        public string Headline {
            get { return Record.Headline; }
            set { Record.Headline = value; }
        }

        public string SubHeadline {
            get { return Record.SubHeadline; }
            set { Record.SubHeadline = value; }
        }

        public string LinkUrl {
            get { return Record.LinkUrl; }
            set { Record.LinkUrl = value; }
        }

        public string GroupName {
            get { return Record.GroupName; }
            set { Record.GroupName = value; }
        }

        public int SlideOrder {
            get { return Record.SlideOrder; }
            set { Record.SlideOrder = value; }
        }
    }
}