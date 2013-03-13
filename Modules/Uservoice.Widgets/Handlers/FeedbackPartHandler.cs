using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using UserVoice.Widgets.Models;

namespace UserVoice.Widgets.Handlers
{
    public class FeedbackPartHandler : ContentHandler
    {
        public FeedbackPartHandler(IRepository<FeedbackPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));

            OnInitializing<FeedbackPart>((context, part) =>
            {
                part.TabPosition = TabPosition.Default;
                part.TabEnabled = true;
            });
        }
    }
}