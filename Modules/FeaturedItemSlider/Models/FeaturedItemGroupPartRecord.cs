using Orchard.ContentManagement.Records;

namespace FeaturedItemSlider.Models {
    public class FeaturedItemGroupPartRecord : ContentPartRecord {
        public virtual string Name { get; set; }
        public virtual int GroupWidth { get; set; }
        public virtual int GroupHeight { get; set; }
        public virtual int ImageWidth { get; set; }
        public virtual int ImageHeight { get; set; }
        public virtual string BackgroundColor { get; set; }
        public virtual string ForegroundColor { get; set; }
        public virtual int SlideSpeed { get; set; }
        public virtual int SlidePause { get; set; }
        public virtual bool ShowSlideNumbers { get; set; }
    }
}