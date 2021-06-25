using Nop.Core.Domain.Catalog;
using Nop.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Filter
{
    public partial interface IFilterMethods : IPlugin
    {
        SpeedFilterSeoModel GetMetaOptions(string filterSeoUrl, Category category);
        bool CategoryFilterEnabled();
        string GetCustomKeyWord(Category category);
        bool IsPermanentRedirect(string FilterUrl, int categoryId);
        string GenerateSpecificationUrl(string FilterUrl, string paramUrl);
    }
}
