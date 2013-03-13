using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.Localization;
using UserVoice.Widgets.Models;
using Orchard.ContentManagement;
using UserVoice.Widgets.ViewModels;
using Orchard.Mvc.Extensions;
using Orchard.UI.Notify;

namespace UserVoice.Widgets.Controllers
{
    public class AdminController : Controller
    {
        private readonly IOrchardServices _services;
        public AdminController(IOrchardServices services)
        {
            _services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Settings()
        {
            var settings = _services.WorkContext.CurrentSite.As<SiteSettingsPart>();

            var viewModel = new SiteSettingsViewModel
            {
                Account = settings.Account,
                Host = settings.Host,
                ApiKey = settings.ApiKey,
            };

            return View(viewModel);
        }

        public ActionResult SaveSettings(string returnUrl)
        {
            var viewModel = new SiteSettingsViewModel();
            TryUpdateModel(viewModel);

            if (ModelState.IsValid)
            {
                var settings = _services.WorkContext.CurrentSite.As<SiteSettingsPart>();

                settings.Account = viewModel.Account;
                settings.Host = viewModel.Host;
                settings.ApiKey = viewModel.ApiKey;

                this._services.Notifier.Information(this.T("UserVoice settings saved"));
            }
            else 
            {
                foreach (var error in ModelState.Values.SelectMany(m => m.Errors).Select(e => e.ErrorMessage))
                {
                    _services.Notifier.Error(T(error));
                }
            }

            return this.RedirectLocal(returnUrl, "~/");
        }
    }
}