﻿using System.Collections.Generic;
using Orchard.ContentManagement.Records;
namespace Contrib.Taxonomies.Models {
    /// <summary>
    /// Represents a Term of a Taxonomy
    /// </summary>
    public class TermPartRecord : ContentPartRecord {
        public virtual int TaxonomyId { get; set; }

        public virtual string Path { get; set; }
        public virtual int Count { get; set; }
        public virtual bool Selectable { get; set; }
        public virtual int Weight { get; set; }

        public virtual IList<TermContentItem> TermContentItems { get; set; }
    }
}
