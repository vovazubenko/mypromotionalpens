using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Filter
{
    public interface IFilterService
    {
        IFilterMethods LoadFilterMethodBySystemName(string systemName);
        SpeedFilterSeoModel GetMetaOptions(string filterSeoUrl, Category category);
        string GetCustomKeyWord(Category category);
        bool IsPermanentRedirect(string FilterUrl, int categoryId);
        string GenerateSpecificationUrl(string filterSeoUrl, string paramUrl);
    }
}
