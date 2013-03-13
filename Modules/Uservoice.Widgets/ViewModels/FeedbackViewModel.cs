using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UserVoice.Widgets.Models;

namespace UserVoice.Widgets.ViewModels
{
    public class FeedbackViewModel
    {
        public string Account { get; set; }
        public string Host { get; set; }
        public string SsoToken { get; set; }

        public string TabLabel { get; set; }
        public string TabColor { get; set; }
        public string TabPosition { get; set; }
        public bool TabInverted { get; set; }
        public bool TabEnabled { get; set; }
        public string WidgetId { get; set; }
    }
}