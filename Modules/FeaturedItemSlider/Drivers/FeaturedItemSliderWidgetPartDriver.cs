using System.Linq;
using FeaturedItemSlider.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace FeaturedItemSlider.Drivers {
    public class FeaturedItemSliderWidgetPartDriver : ContentPartDriver<FeaturedItemSliderWidgetPart> {
        private readonly IContentManager _contentManager;

        public FeaturedItemSliderWidgetPartDriver(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        protected override DriverResult Display(FeaturedItemSliderWidgetPart part, string displayType, dynamic shapeHelper) {
            int slideNumber = 0;

            var featuredItems = _contentManager.Query<FeaturedItemPart, FeaturedItemPartRecord>("FeaturedItem")
                .Where(fip => fip.GroupName == part.GroupName)
                .OrderBy(fi => fi.SlideOrder)
                .List()
                .Select(fi => new FeaturedItemViewModel {
                    Headline = fi.Headline,
                    SubHeadline = fi.SubHeadline,
                    LinkUrl = fi.LinkUrl,
                    ImagePath = fi.Fields.Single(f => f.Name == "Picture").Storage.Get<string>(""),
                    SlideNumber = ++slideNumber
                }).ToList();

            var group = _contentManager.Query<FeaturedItemGroupPart, FeaturedItemGroupPartRecord>("FeaturedItemGroup")
                .Where(fig => fig.Name == part.GroupName)
                .List()
                .SingleOrDefault();

            if (group != null) {
                group.BackgroundColor = group.BackgroundColor.TrimStart('#');
                group.ForegroundColor = group.ForegroundColor.TrimStart('#');
            }

            return ContentShape("Parts_FeaturedItems",
                () => shapeHelper.Parts_FeaturedItems(FeaturedItems: featuredItems, ContentPart: part, Group: group));
        }

        protected override DriverResult Editor(FeaturedItemSliderWidgetPart part, dynamic shapeHelper) {
            var groups = _contentManager.Query<FeaturedItemGroupPart, FeaturedItemGroupPartRecord>("FeaturedItemGroup")
                .List().Select(fig => fig.Name).ToList();

            var viewModel = new FeaturedItemSliderWidgetEditViewModel { GroupNames = groups, SelectedGroup = part.GroupName};
            return ContentShape("Parts_FeaturedItemSliderWidget_Edit",
                () => shapeHelper.EditorTemplate(TemplateName: "Parts.FeaturedItemSliderWidget.Edit", Model: viewModel));
        }

        protected override DriverResult Editor(FeaturedItemSliderWidgetPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, "", null, null);
            return Editor(part, shapeHelper);
        }
    }
}