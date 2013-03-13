using System.Web.Mvc;

namespace Contrib.Taxonomies.ViewModels {
    public class TaxonomyWidgetViewModel {
        public SelectList AvailableTaxonomies { get; set; }
        public int SelectedTaxonomyId { get; set; }
    }
}
