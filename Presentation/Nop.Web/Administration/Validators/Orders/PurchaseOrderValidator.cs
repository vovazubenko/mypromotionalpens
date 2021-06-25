using FluentValidation;
using Nop.Admin.Models.Orders;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Admin.Validators.Orders
{
    public partial class PurchaseOrderValidator : BaseNopValidator<PurchaseOrderModel>
    {
        public PurchaseOrderValidator(ILocalizationService localizationService, IDbContext dbContext)
        {
            RuleFor(x => x.VendorId).NotEmpty().WithMessage(localizationService.GetResource("Admin.Purchaseorder.Fields.VendorId.Required"));
            RuleFor(x => x.VendorEmail).NotEmpty().WithMessage(localizationService.GetResource("Admin.Purchaseorder.Fields.VendorEmail.Required"));
            RuleFor(x => x.VendorEmail).EmailAddress().WithMessage(localizationService.GetResource("Admin.Common.WrongEmail"));
            RuleFor(x => x.PoDate).NotEmpty().WithMessage(localizationService.GetResource("Admin.Purchaseorder.Fields.PoDate.Required"));
            RuleFor(x => x.POShippingCost).NotEmpty().WithMessage(localizationService.GetResource("Admin.Purchaseorder.Fields.POShippingCost.Required"));
            RuleFor(x => x.PONumber).NotEmpty().WithMessage(localizationService.GetResource("Admin.Purchaseorder.Fields.PONumber.Required"));
        }

    }
}