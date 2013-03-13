using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace UserVoice.Widgets.ViewModels
{
    public class FeedbackSettingsViewModel
    {
        public string Account { get; set; }

        [Display(Name = "Label")]
        public string TabLabel { get; set; }

        [Display(Name = "Color")]
        public string TabColor { get; set; }

        [Display(Name = "Position")]
        public string TabPosition { get; set; }

        [Display(Name = "Inverted")]
        public bool TabInverted { get; set; }

        [Display(Name = "Enabled")]
        public bool TabEnabled { get; set; }

        [Display(Name = "Widget")]
        public string WidgetId { get; set; }

        public IList<SelectListItem> TabPositions { get; set; }

        public List<SelectListItem> Widgets { get; set; }
    }
}