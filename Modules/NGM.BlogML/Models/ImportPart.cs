using JetBrains.Annotations;

namespace NGM.BlogML.Models {
    public class ImportPart {
        [CanBeNull]
        public string URLItemPath { get; set; }

        [CanBeNull]
        public string SlugPattern { get; set; }

        [CanBeNull]
        public string DefaultBlogSlug { get; set; }

        [CanBeNull]
        public int? BlogIdToImportInto { get; set; }
    }
}
