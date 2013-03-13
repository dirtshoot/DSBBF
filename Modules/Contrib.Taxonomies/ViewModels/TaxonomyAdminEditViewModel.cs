using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Contrib.Taxonomies.ViewModels {
    public class TaxonomyAdminEditViewModel {
        public int Id { get; set; }
        [Required, DisplayName("Name")]
        public string Name { get; set; }
    }
}
