using Contrib.Taxonomies.Services;
using JetBrains.Annotations;
using Orchard.Environment;
using System.Linq;
using System.Text;

namespace Contrib.Taxonomies.Routing {
    [UsedImplicitly]
    public class TermPathConstraintUpdator : IOrchardShellEvents {
        private readonly ITermPathConstraint _termPathConstraint;
        private readonly ITaxonomyService _taxonomyService;

        public TermPathConstraintUpdator(ITermPathConstraint termPathConstraint, ITaxonomyService taxonomyService) {
            _termPathConstraint = termPathConstraint;
            _taxonomyService = taxonomyService;
        }
        
        void IOrchardShellEvents.Activated() {
            Refresh();
        }

        void IOrchardShellEvents.Terminating() {
        }

        public void Refresh() {
            _termPathConstraint.SetPaths(_taxonomyService.GetTermPaths());
        }
    }
}