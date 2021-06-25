using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Core.Domain.Configuration;
using Nop.Services.Stores;


namespace Nop.Services.Configuration
{
    public partial class RestrictionDomainService : IRestrictionDomainService
    {
        private readonly IRepository<RestrictionDomain> _RestrictionDomainRepository;

        public RestrictionDomainService(IRepository<RestrictionDomain> restrictionDomainRepository)
        {

            this._RestrictionDomainRepository = restrictionDomainRepository;
        }
       public void DeleteDomain(RestrictionDomain restrictionDomain)
        {
            if (restrictionDomain == null)
                throw new ArgumentNullException("RestrictionDomain");

            _RestrictionDomainRepository.Delete(restrictionDomain);
        }
        public void InsertDomain(RestrictionDomain restrictionDomain)
        {
            if (restrictionDomain.Id > 0)
            {
                UpdateDomain(restrictionDomain);
            }
            else
            _RestrictionDomainRepository.Insert(restrictionDomain);

        }
        public void UpdateDomain(RestrictionDomain restrictionDomain) {
            _RestrictionDomainRepository.Update(restrictionDomain);

        }
        public virtual RestrictionDomain GetRestrictionDomainById(int RestrictionDomainId)
        {
            if (RestrictionDomainId == 0)
                return null;

            return _RestrictionDomainRepository.GetById(RestrictionDomainId);
        }
        public virtual IPagedList<RestrictionDomain> GetRestrictionDomain(int pageIndex = 0, int pageSize = int.MaxValue)
        {

            var query = _RestrictionDomainRepository.Table;
            
            query = query.OrderBy(c => c.DomainURL);

            var restrictionDomain = new PagedList<RestrictionDomain>(query, pageIndex, pageSize);
            return restrictionDomain;
            
        }
        public bool IsExists(string DomainURL,int Id) {

            return _RestrictionDomainRepository.Table.Count(d => d.DomainURL.ToLower() == DomainURL.ToLower() && d.Id != Id ) > 0;
        }
        public bool IsDomainRestricted(string DomainURL)
        {

            return _RestrictionDomainRepository.Table.Count(d => d.DomainURL.ToLower() == DomainURL.ToLower()) > 0;
        }
    }

}
