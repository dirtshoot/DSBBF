using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Contrib.Taxonomies.Models {
    public class TaxonomyWidgetPart : ContentPart<TaxonomyWidgetPartRecord> {
        [Required]
        public TaxonomyPartRecord TaxonomyPartRecord {
            get { return Record.TaxonomyPartRecord; }
            set { Record.TaxonomyPartRecord = value; }
        }
    }
}
