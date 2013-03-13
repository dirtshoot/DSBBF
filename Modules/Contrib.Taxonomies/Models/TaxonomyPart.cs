using Orchard.ContentManagement;
using Orchard.Core.Routable.Models;

namespace Contrib.Taxonomies.Models {
    public class TaxonomyPart : ContentPart<TaxonomyPartRecord> {
        public string Name {
            get { return this.As<RoutePart>().Title; }
            set { this.As<RoutePart>().Title = value; }
        }

        public string Slug {
            get { return this.As<RoutePart>().Slug; }
            set { this.As<RoutePart>().Slug = value; }
        }

        public string TermTypeName {
            get { return Record.TermTypeName; }
            set { Record.TermTypeName = value; }
        }
    }
}
