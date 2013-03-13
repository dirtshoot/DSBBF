using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.ContentManagement.Records;

namespace UserVoice.Widgets.Models
{
    public class SuggestionsPartRecord : ContentPartRecord
    {
        public virtual string ForumId { get; set; }
    }
}
