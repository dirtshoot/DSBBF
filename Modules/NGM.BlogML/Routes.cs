using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace NGM.BlogML {
    public class Routes : IRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                             new RouteDescriptor {
                                                     Priority = 15,
                                                     Route = new Route(
                                                         "Admin/Blogs/Import",
                                                         new RouteValueDictionary {
                                                                                      {"area", "NGM.BlogML"},
                                                                                      {"controller", "Admin"},
                                                                                      {"action", "Import"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "NGM.BlogML"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Priority = 15,
                                                     Route = new Route(
                                                         "Admin/Blogs/Export/{id}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "NGM.BlogML"},
                                                                                      {"controller", "Admin"},
                                                                                      {"action", "Export"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "NGM.BlogML"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
                         };
        }
    }
}