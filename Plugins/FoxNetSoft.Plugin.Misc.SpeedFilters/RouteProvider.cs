using System.Web.Routing;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.Routes;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapLocalizedRoute("SpeedFilterCategoryPage",
                            "speedfilters/categorypage/{id}",
                            new { controller = "SpeedFilters", action = "CategoryPage" },
                            new { id = @"\d+"},
                            new[] { "FoxNetSoft.Plugin.Misc.SpeedFilters.Controllers" });
            routes.MapLocalizedRoute("SpeedFilterManufacturerPage",
                            "speedfilters/manufacturerpage/{id}",
                            new { controller = "SpeedFilters", action = "ManufacturerPage" },
                            new { id = @"\d+" },
                            new[] { "FoxNetSoft.Plugin.Misc.SpeedFilters.Controllers" });
            routes.MapLocalizedRoute("SpeedFilterVendorPage",
                            "speedfilters/vendorpage/{id}",
                            new { controller = "SpeedFilters", action = "VendorPage" },
                            new { id = @"\d+" },
                            new[] { "FoxNetSoft.Plugin.Misc.SpeedFilters.Controllers" });

            routes.MapLocalizedRoute("SpeedFilterSearchPage",
                            "speedfilters/searchpage/",
                            new { controller = "SpeedFilters", action = "SearchPage" },
                            new[] { "FoxNetSoft.Plugin.Misc.SpeedFilters.Controllers" });
        }

        public int Priority
        {
            get
            {
                return 1;
            }
        }
    }
}
