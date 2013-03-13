using JetBrains.Annotations;
using Contrib.Taxonomies.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Contrib.Taxonomies.Handlers
{
    [UsedImplicitly]
    public class TaxonomyPartHandler : ContentHandler {
        public TaxonomyPartHandler(IRepository<TaxonomyPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}