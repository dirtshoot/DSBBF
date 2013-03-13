using Contrib.Taxonomies.Services;
using JetBrains.Annotations;
using Orchard.Environment;
using System.Linq;
using System.Text;

namespace Contrib.Taxonomies.Routing {
    [UsedImplicitly]
    public class TaxonomySlugConstraintUpdator : IOrchardShellEvents {
        private readonly ITaxonomySlugConstraint _taxonomySlugConstraint;
        private readonly ITaxonomyService _taxonomyService;

        public TaxonomySlugConstraintUpdator(ITaxonomySlugConstraint taxonomySlugConstraint, ITaxonomyService taxonomyService) {
            _taxonomySlugConstraint = taxonomySlugConstraint;
            _taxonomyService = taxonomyService;
        }
        
        void IOrchardShellEvents.Activated() {
            Refresh();
        }

        void IOrchardShellEvents.Terminating() {
        }

        public void Refresh() {
            _taxonomySlugConstraint.SetSlugs(_taxonomyService.GetSlugs());
        }
    }
}