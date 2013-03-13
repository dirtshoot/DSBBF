using Orchard.Localization;
using Orchard.UI.Navigation;

namespace UserVoice.Widgets {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Settings"),
                menu => menu.Add(T("UserVoice"), item => item.Action("Settings", "Admin", new { area = "UserVoice.Widgets" }))
                    );
        }
    }
}
