using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UserVoice.Widgets.ViewModels
{
    public class SiteSettingsViewModel
    {
        public string Account { get; set; }
        public string Host { get; set; }
        public string ApiKey { get; set; }
    }
}