using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Contrib.Taxonomies.Routing;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Settings;
using Contrib.Taxonomies.Services;
using Orchard.Themes;
using Orchard.UI.Navigation;

namespace Contrib.Taxonomies.Controllers {
    [ValidateInput(false), Themed]
    public class HomeController : Controller {
        private readonly ITaxonomyService _taxonomyService;
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;
        private readonly ITermPathConstraint _termPathConstraint;

        public HomeController(
            ITaxonomyService taxonomyService, 
            IContentManager contentManager, 
            IShapeFactory shapeFactory,
            ISiteService siteService,
            ITermPathConstraint termPathConstraint)
        {
            _taxonomyService = taxonomyService;
            _contentManager = contentManager;
            _siteService = siteService;
            _termPathConstraint = termPathConstraint;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }
        
        dynamic Shape { get; set; }

        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }
        
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ActionResult List(string taxonomySlug, PagerParameters pagerParameters) {
            var taxonomyPart = _taxonomyService.GetTaxonomyBySlug(taxonomySlug);
            if (taxonomyPart == null) {
                return HttpNotFound();
            }

            var terms = _taxonomyService.GetTerms(taxonomyPart.Id).Select(x => _contentManager.BuildDisplay(x.ContentItem, "Summary"));
            dynamic taxonomy = _contentManager.BuildDisplay(taxonomyPart);

            var list = Shape.List();
            list.AddRange(terms);
            taxonomy.Content.Add(list);

            return new ShapeResult(this, taxonomy);
        }

        public ActionResult Item(string termPath, PagerParameters pagerParameters) {
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var correctedPath = _termPathConstraint.FindPath(termPath);
            if (correctedPath == null)
                return HttpNotFound();

            var termPart = _taxonomyService.GetTermByPath(correctedPath);
            if (termPart == null)
                return HttpNotFound();

            var totalItemCount = _taxonomyService.GetContentItemsCount(termPart);

            var termContentItems = _taxonomyService.GetContentItems(termPart, x => true, x => x.Desc( t => t.ContentItemRecord.Id), pager.GetStartIndex(), pager.PageSize)
                .Select(c => _contentManager.BuildDisplay(_contentManager.Get(c.Id), "Summary"));

            dynamic term = _contentManager.BuildDisplay(termPart);

            var list = Shape.List();
            list.AddRange(termContentItems);
            term.Content.Add(Shape.Parts_Taxonomies_TermContentItems_List(ContentItems: list), "5");

            term.Content.Add(Shape.Pager(pager).TotalItemCount(totalItemCount), "Content:after");

            return new ShapeResult(this, term);
        }
    }
}
