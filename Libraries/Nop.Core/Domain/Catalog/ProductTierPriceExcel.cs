using System;
using Ganss.Excel;
using Nop.Core.Domain.Customers;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a tier price
    /// </summary>
    public partial class ProductTierPriceExcel : BaseEntity
    {
        [Column("Product_ID")]
        public int ProductId { get; set; }
        public string Name { get; set; }
        public bool Published { get; set; }
        public string SKU { get; set; }
        public int OrderMinimumQuantity { get; set; }
        public decimal Setup { get; set; }
        [Column("Setup_cost")]
        public decimal SetupCost { get; set; }

        public int QTY1 { get; set; }
        public int QTY2 { get; set; }
        public int QTY3 { get; set; }
        public int QTY4 { get; set; }
        public int QTY5 { get; set; }
        public int QTY6 { get; set; }
        public int QTY7 { get; set; }
        public int QTY8 { get; set; }
        public int QTY9 { get; set; }
        public int QTY10 { get; set; }
        
        public decimal? MSRP1 { get; set; }
        public decimal? MSRP2 { get; set; }
        public decimal? MSRP3 { get; set; }
        public decimal? MSRP4 { get; set; }
        public decimal? MSRP5 { get; set; }
        public decimal? MSRP6 { get; set; }
        public decimal? MSRP7 { get; set; }
        public decimal? MSRP8 { get; set; }
        public decimal? MSRP9 { get; set; }
        public decimal? MSRP10 { get; set; }
        
        public decimal? COST1 { get; set; }
        public decimal? COST2 { get; set; }
        public decimal? COST3 { get; set; }
        public decimal? COST4 { get; set; }
        public decimal? COST5 { get; set; }
        public decimal? COST6 { get; set; }
        public decimal? COST7 { get; set; }
        public decimal? COST8 { get; set; }
        public decimal? COST9 { get; set; }
        public decimal? COST10 { get; set; }
        
        public decimal? PRICE1 { get; set; }
        public decimal? PRICE2 { get; set; }
        public decimal? PRICE3 { get; set; }
        public decimal? PRICE4 { get; set; }
        public decimal? PRICE5 { get; set; }
        public decimal? PRICE6 { get; set; }
        public decimal? PRICE7 { get; set; }
        public decimal? PRICE8 { get; set; }
        public decimal? PRICE9 { get; set; }
        public decimal? PRICE10 { get; set; }
        
        public decimal? DISCOUNT1 { get; set; }
        public decimal? DISCOUNT2 { get; set; }
        public decimal? DISCOUNT3 { get; set; }
        public decimal? DISCOUNT4 { get; set; }
        public decimal? DISCOUNT5 { get; set; }
        public decimal? DISCOUNT6 { get; set; }
        public decimal? DISCOUNT7 { get; set; }
        public decimal? DISCOUNT8 { get; set; }
        public decimal? DISCOUNT9 { get; set; }
        public decimal? DISCOUNT10 { get; set; }
    }
}
