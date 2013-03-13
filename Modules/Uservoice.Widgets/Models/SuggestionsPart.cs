using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace UserVoice.Widgets.Models
{
    public class SuggestionsPart : ContentPart<SuggestionsPartRecord>
    {
        public string ForumId { 
            get { return Record.ForumId; }
            set { Record.ForumId = value; }
        }
    }
}