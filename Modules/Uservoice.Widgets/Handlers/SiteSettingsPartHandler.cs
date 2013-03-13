using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using UserVoice.Widgets.Models;

namespace UserVoice.Widgets.Handlers
{
    public class SiteSettingsPartHandler : ContentHandler
    {
        public SiteSettingsPartHandler(IRepository<SiteSettingsPartRecord> repository)
        {
            Filters.Add(new ActivatingFilter<SiteSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}