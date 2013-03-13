using System;
using System.Web;
using JetBrains.Annotations;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.Services;
using Orchard.Mvc;
using Orchard.Utility.Extensions;
using Szmyd.Orchard.Modules.Sharing.Models;
using Szmyd.Orchard.Modules.Sharing.Settings;
using Szmyd.Orchard.Modules.Sharing.ViewModels;

namespace Szmyd.Orchard.Modules.Sharing.Drivers
{
    [UsedImplicitly]
    public class ShareBarPartDriver : ContentPartDriver<ShareBarPart>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrchardServices _services;

        public ShareBarPartDriver(IHttpContextAccessor httpContextAccessor, IOrchardServices services)
        {
            _httpContextAccessor = httpContextAccessor;
            _services = services;
        }

        protected override DriverResult Display(ShareBarPart part, string displayType, dynamic shapeHelper)
        {
            var shareSettings = _services.WorkContext.CurrentSite.As<ShareBarSettingsPart>();

            // Prevent share bar from showing if account is not set
            if (shareSettings == null || string.IsNullOrWhiteSpace(shareSettings.AddThisAccount))
            {
                return null;
            }

            var routePart = part.As<RoutePart>();
            string containerPath = routePart.GetContainerPath();

            // Prevent share bar from showing when current item is not Routable
            if (routePart.Title != null && routePart.Slug != null)
            {
                var typeSettings = part.Settings.GetModel<ShareBarTypePartSettings>();
                HttpRequestBase request = _httpContextAccessor.Current().Request;
                var containerUrl = new UriBuilder(request.ToRootUrlString()) { Path = (request.ApplicationPath ?? "").TrimEnd('/') + "/" + (containerPath ?? "") };
                var model = new ShareBarViewModel
                {
                    Link = String.Format("{0}/{1}", containerUrl.Uri.ToString().TrimEnd('/'), routePart.Slug),
                    Title = routePart.Title,
                    Account = shareSettings.AddThisAccount,
                    Mode = typeSettings.Mode
                };

                return ContentShape("Parts_Share_ShareBar",
                            () => shapeHelper.Parts_Share_ShareBar(ViewModel: model));



            }

            return null;
        }
    }
}