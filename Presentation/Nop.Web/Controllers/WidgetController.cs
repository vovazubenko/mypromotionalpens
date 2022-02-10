using System.Linq;
using System.Web.Mvc;
using Nop.Web.Factories;

namespace Nop.Web.Controllers
{
    public partial class WidgetController : BasePublicController
    {
		#region Fields

        private readonly IWidgetModelFactory _widgetModelFactory;

        #endregion

        #region Ctor

        public WidgetController(IWidgetModelFactory widgetModelFactory)
        {
            this._widgetModelFactory = widgetModelFactory;
        }

        #endregion

        #region Methods

        [ChildActionOnly]
        public virtual ActionResult WidgetsByZone(string widgetZone, object additionalData = null)
        {
            var model = _widgetModelFactory.GetRenderWidgetModels(widgetZone, additionalData);

            //no data?
            if (!model.Any())
                return Content("");

            return PartialView(model);
        }

        [HttpGet, ActionName("widgets-by-zone-for-logo")]
        public virtual ActionResult WidgetsByZoneForLogo()
        {
            var model = _widgetModelFactory.GetRenderWidgetModels("pavilion_theme_logo", null);
            var data = PartialView("WidgetsByZone", model);

            return data;
        }

        #endregion
    }
}
