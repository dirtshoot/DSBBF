using Orchard.Localization;
using Orchard.UI.Navigation;

namespace NGM.BlogML {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Blog"),
                menu => menu.Add(T("Import"), "10", item => item.Action("Import", "Admin", new { area = "NGM.BlogML" })
                    .Permission(Permissions.ImportBlog)));
        }
    }
}