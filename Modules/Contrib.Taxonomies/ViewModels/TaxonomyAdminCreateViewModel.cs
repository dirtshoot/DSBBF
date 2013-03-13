using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace Contrib.Taxonomies.ViewModels {
    public class TaxonomyAdminCreateViewModel {
        [Required, DisplayName("Name")]
        public string Name { get; set; }
    }
}
