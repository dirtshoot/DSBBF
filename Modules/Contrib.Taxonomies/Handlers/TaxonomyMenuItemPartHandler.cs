using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Contrib.Taxonomies.Models;

namespace Contrib.Taxonomies.Handlers {
    public class TaxonomyMenuItemPartHandler : ContentHandler {
        public TaxonomyMenuItemPartHandler(IRepository<TaxonomyMenuItemPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}