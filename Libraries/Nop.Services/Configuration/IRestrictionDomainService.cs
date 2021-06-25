using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Configuration;

namespace Nop.Services.Configuration
{
    public partial interface IRestrictionDomainService
    {
        void DeleteDomain(RestrictionDomain restrictionDomain);
        void InsertDomain(RestrictionDomain restrictionDomain);
        void UpdateDomain(RestrictionDomain restrictionDomain);
        RestrictionDomain GetRestrictionDomainById(int RestrictionDomainId);
        IPagedList<RestrictionDomain> GetRestrictionDomain(int pageIndex = 0, int pageSize = int.MaxValue);

        bool IsExists(string DomainURL,int Id);
        bool IsDomainRestricted(string DomainURL);
    }
}
