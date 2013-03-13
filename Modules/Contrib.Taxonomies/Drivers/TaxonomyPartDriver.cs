using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Contrib.Taxonomies.Models;

namespace Contrib.Taxonomies.Drivers {
    [UsedImplicitly]
    public class TaxonomyPartDriver : ContentPartDriver<TaxonomyPart> {

        protected override string Prefix { get { return "Taxonomy"; } }

    }
}