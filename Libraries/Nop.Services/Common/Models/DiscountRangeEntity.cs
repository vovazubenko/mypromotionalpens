namespace Nop.Services.Common.Models
{
    public class DiscountRangeEntity
    {
        public int DiscountID { get; set; }

        public string Discount { get; set; }

        public int? MinQty{ get; set; }

        public int? MaxMiniQty { get; set; }

        public decimal Amount { get; set; }
    }
}