using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Security;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Payments
{
    public partial class CreditCardInfoService : ICreditCardInfoService
    {

        #region Fields
        private readonly IRepository<CreditCardInfo> _creditCardInfoRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly CatalogSettings _catalogSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;
        private readonly ICustomerService _customerService;
        private readonly IPluginFinder _pluginFinder;
        private readonly IAddressService _addressService;


        #endregion

        #region Ctor

        public CreditCardInfoService(IRepository<CreditCardInfo> creditCardInfoRepository,
            IAddressService addressService,
            IRepository<StoreMapping> storeMappingRepository,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            IRepository<AclRecord> aclRepository,
            CatalogSettings catalogSettings,
            IEventPublisher eventPublisher,
            ICacheManager cacheManager,
            IPluginFinder pluginFinder,
            ICustomerService customerService)
        {
            this._creditCardInfoRepository = creditCardInfoRepository;
            this._addressService = addressService;
            this._storeMappingRepository = storeMappingRepository;
            this._storeMappingService = storeMappingService;
            this._workContext = workContext;
            this._aclRepository = aclRepository;
            this._catalogSettings = catalogSettings;
            this._eventPublisher = eventPublisher;
            this._cacheManager = cacheManager;
            this._pluginFinder = pluginFinder;
            this._customerService = customerService;
        }

        #endregion

        #region Methods        
        public virtual PaymentInfoResult DeleteCreditCardInfo(CreditCardInfo ccInfo)
        {
            PaymentInfoResult response = new PaymentInfoResult();
            try
            {
                if (ccInfo == null)
                    throw new ArgumentNullException("CreditCardInfo");

                var paymentMethod = LoadPaymentMethodBySystemName("Payments.AuthorizeNet");
                if (paymentMethod == null)
                    throw new NopException("Payment method couldn't be loaded");

                var result = paymentMethod.DeleteCustomerPaymentProfile(ccInfo);
                if (result.Success)
                {
                    _creditCardInfoRepository.Delete(ccInfo);
                    _eventPublisher.EntityDeleted(ccInfo);
                    response.Success = true;
                    response.Error = string.Empty;
                }
                else
                {
                    response.Success = false;
                    response.Error = result.Message;
                }
                //var paymentMethod = LoadPaymentMethodBySystemName("Payments.AuthorizeNet");
                //if (paymentMethod == null)
                //    throw new NopException("Payment method couldn't be loaded");

                //var result = paymentMethod.DeleteCustomerProfile(ccInfo.PaymentProfileId.ToString());
                return response;
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Error = e.InnerException.Message;
                return response;
            }
        }
        public virtual CreditCardInfo GetCreditCardInfoById(int ccInfoId)
        {
            if (ccInfoId == 0)
                return null;

            return _creditCardInfoRepository.GetById(ccInfoId);
        }

        public virtual IList<CreditCardInfo> GetCreditCardInfosByCustomerId(int customerId)
        {
            var query = _creditCardInfoRepository.Table.ToList();
            if (customerId > 0)
                query = query.Where(c => c.CustomerId == customerId).ToList();

            query = query.OrderByDescending(c => c.CreatedDate).ToList();
            return query;
        }
        public virtual PaymentInfoResult InsertCreditCardInfo(CreditCardInfo ccInfo)
        {
            PaymentInfoResult response = new PaymentInfoResult();
            var _encryptionService = EngineContext.Current.Resolve<IEncryptionService>();
            var _paymentService = EngineContext.Current.Resolve<IPaymentService>();
            try
            {

                if (ccInfo == null)
                    throw new ArgumentNullException("CreditCardInfo");

                var paymentMethod = LoadPaymentMethodBySystemName("Payments.AuthorizeNet");
                if (paymentMethod == null)
                    throw new NopException("Payment method couldn't be loaded");


                CustomerPaymentProfileResponse result = new CustomerPaymentProfileResponse();
                CustomerProfileResponse resultCustomerProfile = new CustomerProfileResponse();
                if (!string.IsNullOrEmpty(ccInfo.Customer.CustomerProfileId))
                {
                    result = paymentMethod.CreateCustomerPaymentProfile(ccInfo);
                    if (result.Success)
                    {
                        if (ccInfo.IsDefault)
                        {
                            ccInfo.Customer.CustomerProfileId = Convert.ToString(result.CustomerProfileId);
                            ccInfo.Customer.CustomerPaymentProfileId = Convert.ToString(result.PaymentProfileId);
                            _customerService.UpdateCustomer(ccInfo.Customer);
                        }
                    }
                }
                else {
                    resultCustomerProfile = paymentMethod.CreateCustomerProfile(ccInfo.Customer);
                    if (resultCustomerProfile.Success)
                    {
                        ccInfo.Customer.CustomerProfileId = Convert.ToString(resultCustomerProfile.customerProfileId);
                        _customerService.UpdateCustomer(ccInfo.Customer);

                        result = paymentMethod.CreateCustomerPaymentProfile(ccInfo);
                        if (result.Success)
                        {
                            if (ccInfo.IsDefault)
                            {
                                ccInfo.Customer.CustomerProfileId = Convert.ToString(result.CustomerProfileId);
                                ccInfo.Customer.CustomerPaymentProfileId = Convert.ToString(result.PaymentProfileId);
                                _customerService.UpdateCustomer(ccInfo.Customer);
                            }
                            response.Success = true;
                            response.Error = string.Empty;
                        }
                        else if(!string.IsNullOrEmpty(result.Message))
                        {
                            response.Success = false;
                            response.Error = result.Message;
                        }
                       
                    }
                    else
                    {
                        response.Success = false;
                        response.Error = resultCustomerProfile.message;
                        return response;
                    }
                }

                if (result.Success)
                {
                    string cardNumber = ccInfo.CardNumber;
                    ccInfo.CustomerProfileId = result.CustomerProfileId;
                    ccInfo.PaymentProfileId = result.PaymentProfileId;
                    //ccInfo.CardNumber = (!string.IsNullOrEmpty(ccInfo.CardNumber) ? ("XXXX-XXXX-XXXX-" + ccInfo.CardNumber.Substring(ccInfo.CardNumber.Length - 4)) : string.Empty);
                    //ccInfo.CardNumber = _encryptionService.EncryptText(ccInfo.CardNumber);
                    ccInfo.CardNumber ="";
                    ccInfo.ExpireMonth = 0;
                    ccInfo.ExpireYear = 0;
                    ccInfo.MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(cardNumber));
                    //ccInfo.CVVNumber = _encryptionService.EncryptText(ccInfo.CVVNumber);
                    ccInfo.CVVNumber ="";
                    if (ccInfo.Address == null) {
                        ccInfo.AddressId = null;
                    }
                    if (ccInfo.Address != null)
                    {
                        ccInfo.CreatedDate = DateTime.UtcNow;
                    }
                    ccInfo.CardHolderName = result.PaymentProfile.CardHolderName;
                    _creditCardInfoRepository.Insert(ccInfo);
                    _eventPublisher.EntityInserted(ccInfo);
                    response.Success = true;
                    response.Error = string.Empty;
                    response.CustomerProfileId = result.CustomerProfileId;
                    response.CustomerPaymentProfileId = Convert.ToString(result.PaymentProfileId);
                }
                else
                {
                    
                    response.Success = false;
                    response.Error = result.Message;
                    response.CustomerProfileId = result.CustomerProfileId;
                    response.CustomerPaymentProfileId = Convert.ToString(result.PaymentProfileId);
                }
                return response;
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Error = e.InnerException.Message;
                return response;
            }
        }
        public virtual PaymentInfoResult UpdateCreditCardInfo(CreditCardInfo ccInfo)
        {
            PaymentInfoResult response = new PaymentInfoResult();
            try
            {
                if (ccInfo == null)
                    throw new ArgumentNullException("CreditCardInfo");

                var paymentMethod = LoadPaymentMethodBySystemName("Payments.AuthorizeNet");
                if (paymentMethod == null)
                    throw new NopException("Payment method couldn't be loaded");

                var result = paymentMethod.UpdateCustomerPaymentProfile(ccInfo);
                if (result.Success)
                {
                    if (ccInfo.IsDefault)
                    {
                        ccInfo.Customer.CustomerProfileId = Convert.ToString(ccInfo.CustomerProfileId);
                        ccInfo.Customer.CustomerPaymentProfileId = Convert.ToString(ccInfo.PaymentProfileId);
                        _customerService.UpdateCustomer(ccInfo.Customer);
                    }
                    ccInfo.CustomerProfileId = result.CustomerProfileId;
                    _creditCardInfoRepository.Update(ccInfo);

                    _eventPublisher.EntityUpdated(ccInfo);
                    response.Success = true;
                    response.Error = string.Empty;
                }
                else
                {
                    response.Success = false;
                    response.Error = result.Message;
                }
                return response;
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Error = e.InnerException.Message;
                return response;
            }
        }

        public virtual IPaymentMethod LoadPaymentMethodBySystemName(string systemName)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IPaymentMethod>(systemName);
            if (descriptor != null)
                return descriptor.Instance<IPaymentMethod>();

            return null;
        }

        public virtual void UpdateCreditCardInfoNop(CreditCardInfo ccInfo)
        {
            PaymentInfoResult response = new PaymentInfoResult();
            try
            {
                if (ccInfo == null)
                    throw new ArgumentNullException("CreditCardInfo");

                var paymentMethod = LoadPaymentMethodBySystemName("Payments.AuthorizeNet");
                if (paymentMethod == null)
                    throw new NopException("Payment method couldn't be loaded");

                _creditCardInfoRepository.Update(ccInfo);
                _eventPublisher.EntityUpdated(ccInfo);

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        #endregion
    }
}
