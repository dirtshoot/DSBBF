using JetBrains.Annotations;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Szmyd.Orchard.Modules.Sharing.Models;

namespace Szmyd.Orchard.Modules.Sharing.Handlers
{
    [UsedImplicitly]
    public class ShareBarSettingsPartHandler : ContentHandler
    {
        public ShareBarSettingsPartHandler(IRepository<ShareBarSettingsPartRecord> repository)
        {
            Filters.Add(new ActivatingFilter<ShareBarSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
 }
    }
}