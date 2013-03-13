using System.Web.Routing;
using Contrib.Taxonomies.Services;
using JetBrains.Annotations;
using Contrib.Taxonomies.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Contrib.Taxonomies.Handlers {
    [UsedImplicitly]
    public class TermPartHandler : ContentHandler {
        public TermPartHandler(IRepository<TermPartRecord> repository, ITaxonomyService taxonomyService) {
            Filters.Add(StorageFilter.For(repository));

            OnRemoved<IContent>(
                (context, tags) =>
                    taxonomyService.DeleteAssociatedTerms(context.ContentItem)
                );

            OnInitializing<TermPart>(
                (context, part) => 
                    part.Selectable = true
                );
        }
    }
}