using Orchard.ContentManagement.Records;

namespace Contrib.Taxonomies.Models {
    public class TaxonomyWidgetPartRecord : ContentPartRecord {
        public virtual TaxonomyPartRecord TaxonomyPartRecord { get; set; }
    }
}
