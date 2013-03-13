using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UserVoice.Widgets.ViewModels
{
    public class SuggestionsSettingsViewModel
    {
        public string ForumId { get; set; }

        public IList<SelectListItem> Forums { get; set; }
    }
}