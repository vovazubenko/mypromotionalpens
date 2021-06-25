using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;
using Nop.SigmaSolve.Plugin.Redirects.Infrastructure;

namespace Nop.SigmaSolve.Plugin.Redirects
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            System.Web.Mvc.ViewEngines.Engines.Add(new CustomRedirectionViewEngine());
            var route=routes.MapRoute("Nop.SigmaSolve.Plugin.Redirects.Route",
                 "Admin/Plugin/CusotmRedirects/{action}",
                 new { controller = "CusotmRedirects" },
                 new[] { "Nop.SigmaSolve.Plugin.Redirects.Controllers" }
            );
            route.DataTokens.Add("area", "admin");
            routes.Remove(route);
            routes.Insert(0, route);
           /* routes.MapRoute("{*customAlias}",
                           "{*customAlias}",
                            new RouteValueDictionary {
                                                                                      {"area", "Nop.SigmaSolve.Plugin.Redirects"},
                                                                                      {"controller", "CusotmRedirects"},
                                                                                      {"action", "CustomRedirect"},
                                                                                      {"customAlias", ""}
                                                                                  }); */
        }
        public int Priority
        {
            get
            {
                return -100;
            }
        }
    }

}
