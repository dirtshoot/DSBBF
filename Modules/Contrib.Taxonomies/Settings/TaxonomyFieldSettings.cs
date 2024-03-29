﻿using System.Collections.Generic;
using Contrib.Taxonomies.Models;

namespace Contrib.Taxonomies.Settings {
    public class TaxonomyFieldSettings {
        /// <summary>
        /// Wether the field allows the user to add new Terms to the taxonomy (similar to tags)
        /// </summary>
        public bool AllowCustomTerms { get; set; }

        /// <summary>
        /// The Taxonomy to which this field is related to
        /// </summary>
        public int TaxonomyId { get; set; }

        /// <summary>
        /// Wether the user can only select leaves in the taxonomy
        /// </summary>
        public bool LeavesOnly { get; set; }

        /// <summary>
        /// Wether the user can select only one term or not
        /// </summary>
        public bool SingleChoice { get; set; }

        public IEnumerable<TaxonomyPart> Taxonomies { get; set; }
    }
}