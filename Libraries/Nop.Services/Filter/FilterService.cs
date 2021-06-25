using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Filter
{
    public partial class FilterService : IFilterService
    {
        private readonly IPluginFinder _pluginFinder;

        public FilterService(IPluginFinder pluginFinder)
        {
            this._pluginFinder= pluginFinder;
        }
        public virtual IFilterMethods LoadFilterMethodBySystemName(string systemName)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IFilterMethods>(systemName);
            if (descriptor != null)
                return descriptor.Instance<IFilterMethods>();

            return null;
        }

        public virtual SpeedFilterSeoModel GetMetaOptions(string filterSeoUrl, Category category)
        {
            var filterMethod = LoadFilterMethodBySystemName("FoxNetSoft.Plugin.Misc.SpeedFilters");
            if (filterMethod == null)
                return new SpeedFilterSeoModel();
            return filterMethod.GetMetaOptions(filterSeoUrl, category);
        }

        public virtual string GetCustomKeyWord(Category category) {
            var filterMethod = LoadFilterMethodBySystemName("FoxNetSoft.Plugin.Misc.SpeedFilters");
            if (filterMethod == null)
                return "";
            return filterMethod.GetCustomKeyWord(category);
        }
        
        public bool IsPermanentRedirect(string FilterUrl, int categoryId)
        {
            var filterMethod = LoadFilterMethodBySystemName("FoxNetSoft.Plugin.Misc.SpeedFilters");
            if (filterMethod == null)
                return false;
            return filterMethod.IsPermanentRedirect(FilterUrl,categoryId);
        }

        public string GenerateSpecificationUrl(string filterSeoUrl,string paramUrl) {
            var filterMethod = LoadFilterMethodBySystemName("FoxNetSoft.Plugin.Misc.SpeedFilters");
            if (filterMethod == null)
                return "";
            return filterMethod.GenerateSpecificationUrl(filterSeoUrl, paramUrl);
        }
    }
}
