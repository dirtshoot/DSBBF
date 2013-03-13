using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace UserVoice.Widgets.Models
{
    public class SiteSettingsPart : ContentPart<SiteSettingsPartRecord>
    {
        public string Account
        {
            get { return Record.Account; }
            set { Record.Account = value;  }
        }

        public string Host
        {
            get { return Record.Host; }
            set { Record.Host = value; }
        }

        public string ApiKey
        {
            get { return Record.ApiKey; }
            set { Record.ApiKey = value; }
        }

    }
}