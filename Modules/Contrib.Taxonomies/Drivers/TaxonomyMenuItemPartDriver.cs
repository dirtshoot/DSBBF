using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Contrib.Taxonomies.Models;
using Orchard.Localization;

namespace Contrib.Taxonomies.Drivers {
    [UsedImplicitly]
    public class TaxonomyMenuItemPartDriver : ContentPartDriver<TaxonomyMenuItemPart> {

        public TaxonomyMenuItemPartDriver() {
            T = NullLocalizer.Instance;
        }

        Localizer T { get; set; }

        protected override string Prefix { get { return "TaxonomyMenuItemPart"; } }

        protected override DriverResult Editor(TaxonomyMenuItemPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Taxonomies_TaxonomyMenuItem_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: "Parts/Taxonomies.TaxonomyMenuItem", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(TaxonomyMenuItemPart menuItemPart, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(menuItemPart, Prefix, null, null);
            return Editor(menuItemPart, shapeHelper);
        }
    }
}