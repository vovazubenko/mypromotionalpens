using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.SigmaSolve.Plugin.Redirects.Domain;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nop.SigmaSolve.Plugin.Redirects.Extentions;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework;
using Nop.Core.Infrastructure;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;
using System.Web.Mvc;


namespace Nop.SigmaSolve.Plugin.Redirects.Controllers
{
    public class CusotmRedirectsController : BasePluginController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly ICacheManager _cacheManager;
        private readonly ILocalizationService _localizationService;
        private readonly IRepository<CustomRedirection> _repositoryCustomRedirects;
        public CusotmRedirectsController(IWorkContext workContext,
            IStoreContext storeContext,
            IStoreService storeService,
            ISettingService settingService,
            ICacheManager cacheManager,
            ILocalizationService localizationService,
            IRepository<CustomRedirection> repositoryCustomRedirects)
        {
            this._repositoryCustomRedirects = repositoryCustomRedirects;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._cacheManager = cacheManager;
            this._localizationService = localizationService;
        }

        #region Website Owner Edit,Update,Listing
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddUpdate(CustomRedirection model)
        {
            if (ModelState.IsValid)
            {
                CustomRedirection record = new CustomRedirection();
                if (model.Id != 0)
                    record = _repositoryCustomRedirects.GetById(model.Id);
                record.Alias = model.Alias;
                record.RedirectTo = model.RedirectTo;
                record.IsEnabled = model.IsEnabled;
                record.PermanentRedirect = model.PermanentRedirect;
                if (model.Id != 0)
                    _repositoryCustomRedirects.Update(record);
                else
                    _repositoryCustomRedirects.Insert(record);
                _cacheManager.Clear();
            }
            else
            {
                var modelErrors = ModelState.AllErrors();
                var gridModel = new DataSourceResult
                {
                    Errors = modelErrors
                };
                return Json(gridModel);
            }
            return new NullJsonResult();

        }

        [HttpPost]
        public ActionResult Delete(CustomRedirection model)
        {
            if (ModelState.IsValid)
            {
                CustomRedirection s = _repositoryCustomRedirects.GetById(model.Id);
                if (s != null)
                    _repositoryCustomRedirects.Delete(s);
                _cacheManager.Clear();
            }
            return new NullJsonResult();
        }
        [HttpPost]
        public ActionResult Records(DataSourceRequest command, Nop.Web.Framework.Kendoui.Filter filter = null, IEnumerable<Sort> sort = null)
        {
            var records = _repositoryCustomRedirects.Table.ToList();
            var resources = records
                .Select(x => new CustomRedirection
                {
                    Id = x.Id,
                    Alias = x.Alias,
                    RedirectTo = x.RedirectTo,
                    PermanentRedirect = x.PermanentRedirect,
                    IsEnabled = x.IsEnabled
                })
                .AsQueryable()
                .Filter(filter)
                .Sort(sort);

            var gridModel = new DataSourceResult
            {
                Data = resources.PagedForCommand(command),
                Total = resources.Count()
            };
            return Json(gridModel);

        }
        #endregion
        public ActionResult CustomRedirect(string customAlias)
        {
            // I placed code for redirection in global.apsx as noopcommerce has some restrictions
            if (customAlias == null)
                customAlias = "";
            var link = _repositoryCustomRedirects.Table.Where(x => x.Alias != null && x.Alias.ToLower() == customAlias.ToLower()).FirstOrDefault();
            if (link != null)
                return new RedirectResult(link.RedirectTo, link.PermanentRedirect);
            return RedirectToAction("PageNotFound", "Common");
        }
    }
}
