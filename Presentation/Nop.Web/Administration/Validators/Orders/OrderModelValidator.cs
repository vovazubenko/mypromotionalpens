using FluentValidation;
using Nop.Admin.Models.Orders;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Admin.Validators.Orders
{
    public partial class OrderModelValidator : BaseNopValidator<OrderModel>
    {
        public OrderModelValidator(ILocalizationService localizationService, IDbContext dbContext)
        {
            RuleFor(x => x.TransactionNote).NotEmpty().WithMessage(localizationService.GetResource("Admin.Order.Fields.TransactionNote.Required"));
        }
    }
}