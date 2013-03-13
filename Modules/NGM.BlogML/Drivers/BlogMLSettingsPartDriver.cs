using NGM.BlogML.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace NGM.BlogML.Drivers {
    public class BlogMLSettingsPartDriver : ContentPartDriver<BlogMLSettingsPart> {
        public BlogMLSettingsPartDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "BlogMLSettings"; } }

        protected override DriverResult Editor(BlogMLSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(BlogMLSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {

            return ContentShape("Parts_BlogML_SiteSettings", () => {
                if (updater != null) {
                    updater.TryUpdateModel(part.Record, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: "Parts.BlogML.SiteSettings", Model: part.Record, Prefix: Prefix);
            })
                .OnGroup("blogml");
        }
    }
}