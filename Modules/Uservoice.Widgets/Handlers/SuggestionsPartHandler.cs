using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using UserVoice.Widgets.Models;

namespace UserVoice.Widgets.Handlers
{
    public class SuggestionsPartHandler : ContentHandler
    {
        public SuggestionsPartHandler(IRepository<SuggestionsPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}