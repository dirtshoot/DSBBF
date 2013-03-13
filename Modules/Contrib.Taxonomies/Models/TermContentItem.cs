using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Contrib.Taxonomies.Models {
    /// <summary>
    /// Represents a relationship between a Term and a Content Item
    /// </summary>
    public class TermContentItem {
        public virtual int Id { get; set; }
        public virtual string Field { get; set; }
        
        [CascadeAllDeleteOrphan]
        public virtual TermPartRecord TermRecord { get; set; }

        [CascadeAllDeleteOrphan]
        public virtual ContentItemRecord ContentItemRecord { get; set; }
    }
}