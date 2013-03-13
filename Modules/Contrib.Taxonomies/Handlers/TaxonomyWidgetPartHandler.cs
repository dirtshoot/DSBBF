using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Contrib.Taxonomies.Models;

namespace Contrib.Taxonomies.Handlers {
    [UsedImplicitly]
    public class TaxonomyWidgetPartHandler : ContentHandler {
        public TaxonomyWidgetPartHandler(IRepository<TaxonomyWidgetPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}