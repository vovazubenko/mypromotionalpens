using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Security;
using Nop.Core.Plugins;
using Nop.SigmaSolve.Plugin.Redirects.Data;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;

namespace Nop.SigmaSolve.Plugin.Redirects
{
    public class PluginConfigurations : BasePlugin, IAdminMenuPlugin
    {
        private PluginContext _context;

        #region Fields

        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        #endregion

        #region Ctor

        public PluginConfigurations(IStoreContext storeContext, PluginContext context, ISettingService settingService, IPermissionService permissionService, ICustomerService customerService)
        {
            this._storeContext = storeContext;
            this._context = context;
            this._settingService = settingService;
            this._permissionService = permissionService;
            this._customerService = customerService;
        }

        #endregion


        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Authenticate()
        {
            return true;
        }
        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                IconClass = "fa-dot-circle-o",
                SystemName = "CustomRedirection",
                Title = "Redirects",
                ControllerName = "CusotmRedirects",
                ActionName = "Index",
                Visible = true,
                RouteValues = new RouteValueDictionary() { { "Area", "Admin" } },
            };
            var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Configuration");
            if (pluginNode != null)
                pluginNode.ChildNodes.Add(menuItem);
            else
                rootNode.ChildNodes.Add(menuItem);
        }
        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Index";
            controllerName = "CusotmRedirects";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.SigmaSolve.Plugin.Redirects.Controllers" }, { "area", null } };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public SiteMapNode BuildMenuItem()
        {
            return new SiteMapNode();
        }


        /// <summary>
        /// Gets a route for displaying plugin in public store
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPublicInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Index";
            controllerName = "CusotmRedirects";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.SigmaSolve.Plugin.Redirects.Controllers" }, { "area", null } };
        }
        public void AddPermissionToCustomerRole(string systemName, string customerRoleName)
        {
            var role = _customerService.GetCustomerRoleBySystemName(customerRoleName);
            if (role != null)
            {
                var permission = _permissionService.GetPermissionRecordBySystemName(systemName);
                if (permission != null)
                {
                    if (!permission.CustomerRoles.Contains(role))
                    {
                        permission.CustomerRoles.Add(role);
                        _permissionService.UpdatePermissionRecord(permission);
                    }
                }
            }

        }
        public void AddPermission(string systemName, string Name, string caregory)
        {
            PermissionRecord permission = new PermissionRecord();
            permission.SystemName = systemName;
            permission.Name = Name;
            permission.Category = caregory;
            _permissionService.InsertPermissionRecord(permission);
            AddPermissionToCustomerRole(systemName, "Administrators");
        }
        public void DeletePermission(string category, string systemName)
        {
            var permissions = _permissionService.GetAllPermissionRecords().Where(x => x.Category == category && x.SystemName == systemName).ToList();
            foreach (var permission in permissions)
                _permissionService.DeletePermissionRecord(permission);
        }
        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            _context.Install();
            // this.AddOrUpdatePluginLocaleResource("Nop.Plugin.WebSiteOwner.Models.PluginSettings.AddingZoneName", "Display Zone Name");
            // this.AddOrUpdatePluginLocaleResource("Nop.Plugin.WebSiteOwner.Models.PluginSettings.RemarketingPixelCode", "Remarketing Pixel Code");
            base.Install();
        }

        public override void Uninstall()
        {
            _context.Uninstall();
            // DeletePermission("Plugin", "ManageAutorizeNetPaymentSystemPayments");
            //settings
            //this.DeletePluginLocaleResource("Nop.Plugin.WebSiteOwner.Models.PluginSettings.AddingZoneName");
            //this.DeletePluginLocaleResource("Nop.Plugin.WebSiteOwner.Models.PluginSettings.RemarketingPixelCode");
            base.Uninstall();
        }

        #endregion
    }
}
