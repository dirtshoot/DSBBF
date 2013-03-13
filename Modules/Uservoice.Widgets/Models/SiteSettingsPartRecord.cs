using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Records;

namespace UserVoice.Widgets.Models
{
    public class SiteSettingsPartRecord : ContentPartRecord
    {
        public virtual string Account { get; set; }
        public virtual string Host { get; set; }
        public virtual string ApiKey { get; set; }
    }
}