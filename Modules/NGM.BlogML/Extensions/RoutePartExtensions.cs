using System.Text.RegularExpressions;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.Services;

namespace NGM.BlogML.Extensions {
    public static class RoutePartExtensions {
        public static RoutePart Slugify(this RoutePart routePart, IRoutableService routableService, string slugPattern, string postUrl) {
            if (!string.IsNullOrEmpty(slugPattern) && (!string.IsNullOrEmpty(postUrl))) {
                var match = Regex.Match(postUrl, slugPattern);
                if (match.Groups.Count >= 2) {
                    routePart.Slug = match.Groups[1].Value;
                    routePart.Path = routePart.GetPathWithSlug(routePart.Slug);
                    return routePart;
                }

                routableService.FillSlugFromTitle(routePart);
                routePart.Path = routePart.GetPathWithSlug(routePart.Slug);
                return routePart;
            }

            if (!string.IsNullOrEmpty(postUrl))
                routePart.Slug = postUrl;
            else
                routableService.FillSlugFromTitle(routePart);

            routePart.Path = routePart.GetPathWithSlug(routePart.Slug);

            return routePart;
        }
    }
}