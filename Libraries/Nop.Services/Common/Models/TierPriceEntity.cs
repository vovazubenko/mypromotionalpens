namespace Nop.Services.Common.Models
{
    public class TierPriceEntity
    {
        public int Id { get; set; }
        public decimal PriceBase { get; set; }
        public string Price { get; set; }
        public int Quantity { get; set; }
        public decimal MSRP { get; set; } = 0M;
    }
}