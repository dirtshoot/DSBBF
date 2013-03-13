using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace FeaturedItemSlider.Models {
    public class FeaturedItemGroupPart : ContentPart<FeaturedItemGroupPartRecord> {
        private const int MinFeatureDimension = 1;
        private const int MaxFeatureDimension = 99999;

        [Required]
        public string Name {
            get { return Record.Name; }
            set { Record.Name = value; }
        }

        [Range(MinFeatureDimension, MaxFeatureDimension, ErrorMessage = "Group Width must be greater than zero.")]
        public int GroupWidth {
            get { return Record.GroupWidth; }
            set { Record.GroupWidth = value; }
        }

        [Range(MinFeatureDimension, MaxFeatureDimension, ErrorMessage = "Group Height must be greater than zero.")]
        public int GroupHeight {
            get { return Record.GroupHeight; }
            set { Record.GroupHeight = value; }
        }

        [Range(MinFeatureDimension, MaxFeatureDimension, ErrorMessage = "Image Width must be greater than zero.")]
        public int ImageWidth {
            get { return Record.ImageWidth; }
            set { Record.ImageWidth = value; }
        }

        [Range(MinFeatureDimension, MaxFeatureDimension, ErrorMessage = "Image Height must be greater than zero.")]
        public int ImageHeight {
            get { return Record.ImageHeight; }
            set { Record.ImageHeight = value; }
        }

        [Required]
        public string BackgroundColor {
            get { return Record.BackgroundColor; }
            set { Record.BackgroundColor = value; }
        }

        [Required]
        public string ForegroundColor {
            get { return Record.ForegroundColor; }
            set { Record.ForegroundColor = value; }
        }

        [Range(1, int.MaxValue, ErrorMessage = "Slide Speed must at least one millisecond.")]
        public int SlideSpeed {
            get { return Record.SlideSpeed; }
            set { Record.SlideSpeed = value; }
        }

        [Range(15, int.MaxValue, ErrorMessage = "Slide Pause must be at least fifteen milliseconds.")]
        public int SlidePause {
            get { return Record.SlidePause; }
            set { Record.SlidePause = value; }
        }

        public bool ShowSlideNumbers {
            get { return Record.ShowSlideNumbers; }
            set { Record.ShowSlideNumbers = value; }
        }
    }
}