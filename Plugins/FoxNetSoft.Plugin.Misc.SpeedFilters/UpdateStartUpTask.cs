using FoxNetSoft.Plugin.Misc.SpeedFilters.Data;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Logger;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Services.Configuration;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters
{
    public class UpdateStartUpTask : IStartupTask
    {
        public void Execute()
        {
            var speedFiltersSettings = EngineContext.Current.Resolve<SpeedFiltersSettings>();
            if (speedFiltersSettings != null)
            {
                IPluginFinder pluginFinder = EngineContext.Current.Resolve<IPluginFinder>();
                var pluginDescriptor = pluginFinder.GetPluginDescriptorBySystemName("FoxNetSoft.Plugin.Misc.SpeedFilters");
                if (pluginDescriptor == null)
                    return;
                if (!pluginDescriptor.Installed)
                    return;
                var _settingService = EngineContext.Current.Resolve<ISettingService>();
                if (_settingService == null)
                    return;
                var objectContext = EngineContext.Current.Resolve<SpeedFiltersObjectContext>();
                if (objectContext == null)
                    return;

                //105
                if (speedFiltersSettings.Version < 105)
                {
                    speedFiltersSettings.Version = 105;
                    _settingService.SaveSetting(speedFiltersSettings);

                    //update SpeedFiltersObjectContext
                    objectContext.UpdateInstallationScript(speedFiltersSettings.Version);
                }
                //106
                if (speedFiltersSettings.Version < 106)
                {
                    speedFiltersSettings.AllowSelectFiltersInOneBlock = true;
                    speedFiltersSettings.Version = 106;
                    _settingService.SaveSetting(speedFiltersSettings);

                    //update SpeedFiltersObjectContext
                    objectContext.UpdateInstallationScript(speedFiltersSettings.Version);
                }
                //114
                if (speedFiltersSettings.Version < 114)
                {
                    speedFiltersSettings.Version = 114;
                    _settingService.SaveSetting(speedFiltersSettings);
                    objectContext.UpdateInstallationScript(speedFiltersSettings.Version);
                }
            }
        }

        public int Order
        {
            //ensure that this task is run first 
            get { return 100; }
        }
    }
}
