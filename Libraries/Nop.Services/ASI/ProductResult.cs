using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.ASI
{
    public class Phone
    {
        public string Work { get; set; }
        public string TollFree { get; set; }
        public string Primary { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }
    }

    public class Fax
    {
        public string Work { get; set; }
        public string Primary { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }
    }

    public class Rating
    {
        [JsonProperty("Rating")]
        public int RatingData { get; set; }
        public int Companies { get; set; }
        public int Transactions { get; set; }
    }

    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AsiNumber { get; set; }
        public Phone Phone { get; set; }
        public Fax Fax { get; set; }
        public List<string> Websites { get; set; }
        public Rating Rating { get; set; }
        public string MarketingPolicy { get; set; }
        public bool HasNotes { get; set; }
    }

    public class Issue
    {
        public int Id { get; set; }
    }

    public class Page
    {
        public int Id { get; set; }
        public string Number { get; set; }
    }

    public class Catalog
    {
        public string Name { get; set; }
        public string Year { get; set; }
        public Issue Issue { get; set; }
        public List<Page> Pages { get; set; }
    }

    public class Category
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public Parent Parent { get; set; }
    }
    public class ProductionTimeDays
    {
        public string From { get; set; }
        public string To { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }
    }
    public class ProductionTime
    {
        public string Name { get; set; }
        public string Description { get; set; }
        private object days;
        public object Days
        {
            get
            {
                return this.days;
            }
            set
            {
                int j = 0;
                if (int.TryParse(value.ToString(), out j))
                {
                    this.days = j;
                }
                else
                {
                    try
                    {
                        this.days = (value as JObject).ToObject<ProductionTimeDays>();
                    }
                    catch (Exception e)
                    {
                        this.days = value;
                    }
                }
            }
        }
    }

    public class Quantity
    {
        public int From { get; set; }
        public long To { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }
    }

    public class Price
    {
        public Quantity Quantity { get; set; }
        [JsonProperty("Price")]
        public double PriceData { get; set; }
        public double Cost { get; set; }
        public string DiscountCode { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsQUR { get; set; }
    }

    public class Charges
    {
        public string TypeCode { get; set; }
        public string Type { get; set; }
        public List<Price> Prices { get; set; }
        public string PriceIncludes { get; set; }
    }

    public class RushTime
    {
        public string Name { get; set; }
        public string Description { get; set; }
        private object days;
        public object Days
        {
            get
            {
                return this.days;
            }
            set
            {
                int j = 0;
                if (int.TryParse(value.ToString(), out j))
                {
                    this.days = j;
                }
                else
                {
                    try
                    {
                        this.days = (value as JObject).ToObject<ProductionTimeDays>();
                    }
                    catch (Exception e)
                    {
                        this.days = value;
                    }
                }
            }
        }
        public Charges Charges { get; set; }
    }

    public class Value
    {
        public string Code { get; set; }
        public string Name { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }
    }

    public class Sizes
    {
        public object ValuesConverted { get; set; }
        public object Values
        {
            get
            {
                return this.ValuesConverted;
            }
            set
            {
                if (value.GetType() == typeof(JArray))
                {
                    try
                    {
                        var data = (JArray)value;
                        foreach (var item in data)
                        {
                            if (this.ValuesConverted == null)
                            {
                                try
                                {
                                    var i = (item as JObject).ToObject<Value>();
                                    this.ValuesConverted = new List<Value>();
                                }
                                catch (Exception ei)
                                {
                                    this.ValuesConverted = new List<string>();
                                }
                            }
                            if (this.ValuesConverted is List<Value>)
                            {
                                var i = (item as JObject).ToObject<Value>();
                                (this.ValuesConverted as List<Value>).Add(i);
                            }
                            else
                            {
                                var i = item.ToString();
                                (this.ValuesConverted as List<string>).Add(i);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        this.ValuesConverted = value;
                    }
                }
                else
                    this.ValuesConverted = value;
            }
        }

    }

    public class Value2
    {
        public string Code { get; set; }
        public string Name { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }
    }

    public class Materials
    {
        public List<Value2> Values { get; set; }
    }

    public class Attributes
    {
        public Colors Colors { get; set; }
        public Sizes Sizes { get; set; }
        public Materials Materials { get; set; }
        #region SP
        public Shapes Shapes { get; set; }
        #endregion
    }

    #region SP
    public class ShapeValue
    {
        public string Code { get; set; }
        public string Name { get; set; }
        [JsonProperty("$index")]
        public int index { get; set; }
        public string ImageUrl { get; set; }
    }
    public class Shapes
    {
        public List<ShapeValue> Values { get; set; }
    }

    public class Locations
    {
        public List<object> Values { get; set; }
    }
    #endregion
    public class Value3
    {
        public string Code { get; set; }
        public string Name { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }
    }

    public class Colors
    {
        public List<Value3> Values { get; set; }
    }

    public class Quantity2
    {
        public int From { get; set; }
        public object To { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }
    }

    public class Price2
    {
        public Quantity2 Quantity { get; set; }
        public double Price { get; set; }
        public double Cost { get; set; }
        public string DiscountCode { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsQUR { get; set; }
    }

    public class Charge
    {
        public string TypeCode { get; set; }
        public string Type { get; set; }
        public List<Price2> Prices { get; set; }
        public string PriceIncludes { get; set; }
        public string Name { get; set; }
        public string UsageLevelCode { get; set; }
        public string UsageLevel { get; set; }
    }

    public class Value4
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public List<Charge> Charges { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }
        public string Description { get; set; }
    }

    public class Methods
    {
        public List<Value4> Values { get; set; }
    }

    public class Quantity3
    {
        public int From { get; set; }
        public object To { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }
    }

    public class Price3
    {
        public Quantity3 Quantity { get; set; }
        public double Price { get; set; }
        public double Cost { get; set; }
        public string DiscountCode { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsQUR { get; set; }
    }

    public class Charge2
    {
        public string TypeCode { get; set; }
        public string Type { get; set; }
        public List<Price3> Prices { get; set; }
        public string PriceIncludes { get; set; }
    }

    public class Value5
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Charge2> Charges { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }
    }

    public class Services
    {
        public List<Value5> Values { get; set; }
    }

    public class Value6
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }
        public List<SizeOptions> Options { get; set; }
    }
    public class SizeOptions
    {
        public string Type { get; set; }
        public List<string> Values { get; set; }
        public string Name { get; set; }
        public List<SizeGroup> Groups { get; set; }
    }
    public class SizeGroup
    {
        public string Name { get; set; }
        public List<SizeCharge> Charges { get; set; }
    }
    public class SizeCharge
    {
        public string Name { get; set; }
        public string TypeCode { get; set; }
        public string Type { get; set; }
        public List<SizePrice> Prices { get; set; }
        public string UsageLevelCode { get; set; }
        public string UsageLevel { get; set; }
    }
    public class SizePrice
    {
        public Quantity2 Quantity { get; set; }
        public double Price { get; set; }
        public double Cost { get; set; }
        public string DiscountCode { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsQUR { get; set; }
    }
    public class Sizes2
    {
        public List<Value6> Values { get; set; }
    }

    public class Quantity4
    {
        public int From { get; set; }
        public object To { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }
    }

    public class Price4
    {
        public Quantity4 Quantity { get; set; }
        public string DiscountCode { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsQUR { get; set; }
        public double? Price { get; set; }
        public double? Cost { get; set; }
    }

    public class Charge3
    {
        public string TypeCode { get; set; }
        public string Type { get; set; }
        public List<Price4> Prices { get; set; }
        public string PriceIncludes { get; set; }
    }

    public class Quantity5
    {
        public int From { get; set; }
        public object To { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }
    }

    public class Price5
    {
        public Quantity5 Quantity { get; set; }
        public double Price { get; set; }
        public double Cost { get; set; }
        public string DiscountCode { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsQUR { get; set; }
    }

    public class Charge4
    {
        public string Name { get; set; }
        public string TypeCode { get; set; }
        public string Type { get; set; }
        public List<Price5> Prices { get; set; }
        public string UsageLevelCode { get; set; }
        public string UsageLevel { get; set; }
        public string PriceIncludes { get; set; }
    }

    public class Group
    {
        public string Name { get; set; }
        public List<Charge4> Charges { get; set; }
    }

    public class Option
    {
        public string Name { get; set; }
        public List<object> Values { get; set; }
        public string Type { get; set; }
        public List<Charge3> Charges { get; set; }
        public List<Group> Groups { get; set; }
    }

    public class Imprinting
    {
        public Colors Colors { get; set; }
        public Methods Methods { get; set; }
        public Services Services { get; set; }
        public Sizes2 Sizes { get; set; }
        public List<Option> Options { get; set; }
        public bool FullColorProcess { get; set; }
        public bool Personalization { get; set; }
        public bool SoldUnimprinted { get; set; }

        #region sp
        public Locations Locations { get; set; }
        #endregion
    }

    public class Quantity6
    {
        public int From { get; set; }
        public long To { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }
    }

    public class Price6
    {
        public Quantity6 Quantity { get; set; }
        public double Price { get; set; }
        public double Cost { get; set; }
        public string DiscountCode { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsQUR { get; set; }
    }

    public class Charge5
    {
        public string TypeCode { get; set; }
        public string Type { get; set; }
        public List<Price6> Prices { get; set; }
        public string UsageLevelCode { get; set; }
        public string UsageLevel { get; set; }
    }

    public class Option2
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public List<Charge5> Charges { get; set; }
        public object Values { get; set; }
    }

    public partial class Packaging
    {
        public string Type { get; set; }
        public object Values { get; set; }
    }

    public class Value7
    {
        public string Code { get; set; }
        public string Name { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }
    }

    public class FOBPoints
    {
        public List<Value7> Values { get; set; }
    }
    public class ShippingWeightValues
    {
        public string Code { get; set; }
        public string Name { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }

    }
    public class Weight
    {
        public object ValuesConverted { get; set; }
        public object Values
        {
            get
            {
                return this.ValuesConverted;
            }
            set
            {
                if (value.GetType() == typeof(JArray))
                {
                    try
                    {
                        var data = (JArray)value;
                        foreach (var item in data)
                        {
                            if (this.ValuesConverted == null)
                            {
                                try
                                {
                                    var i = (item as JObject).ToObject<ShippingWeightValues>();
                                    this.ValuesConverted = new List<ShippingWeightValues>();
                                }
                                catch (Exception ei)
                                {
                                    this.ValuesConverted = new List<string>();
                                }
                            }
                            if (this.ValuesConverted is List<ShippingWeightValues>)
                            {
                                var i = (item as JObject).ToObject<ShippingWeightValues>();
                                (this.ValuesConverted as List<ShippingWeightValues>).Add(i);
                            }
                            else
                            {
                                var i = item.ToString();
                                (this.ValuesConverted as List<string>).Add(i);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        this.ValuesConverted = value;
                    }
                }
                else
                    this.ValuesConverted = value;
            }
        }
    }

    public class Shipping
    {
        public FOBPoints FOBPoints { get; set; }
        public Weight Weight { get; set; }
        public bool BillsByWeight { get; set; }
        public bool BillsBySize { get; set; }

        public Dimensions Dimensions { get; set; }
        public decimal WeightPerPackage { get; set; }
        public int ItemsPerPackage { get; set; }

        
    }

    public class Quantity7
    {
        public int From { get; set; }
        public long? To { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }
    }

    public class Price7
    {
        public Quantity7 Quantity { get; set; }
        public double Price { get; set; }
        public double Cost { get; set; }
        public string DiscountCode { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsQUR { get; set; }
    }

    public class Quantity8
    {
        public int From { get; set; }
        public long To { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }
    }

    public class LowestPrice
    {
        public Quantity8 Quantity { get; set; }
        public double Price { get; set; }
        public double Cost { get; set; }
        public string DiscountCode { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsQUR { get; set; }
    }

    public class Quantity9
    {
        public int From { get; set; }
        public int To { get; set; }
        [JsonProperty("$index")]
        public int Index { get; set; }
    }

    public class HighestPrice
    {
        public Quantity9 Quantity { get; set; }
        public double Price { get; set; }
        public double Cost { get; set; }
        public string DiscountCode { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsQUR { get; set; }
    }
    public class VirtualSampleImages
    {
        public int? Id { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool? IsPrimary { get; set; }
        public bool? IsVirtualSample { get; set; }
    }

    public class Parent
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class Result
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string Number { get; set; }
        public string ImageUrl { get; set; }
        public List<string> Images { get; set; }
        public List<VirtualSampleImages> VirtualSampleImages { get; set; }
        public Supplier Supplier { get; set; }
        public List<string> LineNames { get; set; }
        public Catalog Catalog { get; set; }
        public List<Category> Categories { get; set; }
        public List<string> Themes { get; set; }
        public List<string> Origin { get; set; }
        public List<ProductionTime> ProductionTime { get; set; }
        public List<RushTime> RushTime { get; set; }
        public Attributes Attributes { get; set; }
        public Imprinting Imprinting { get; set; }
        public List<Option2> Options { get; set; }
        public List<Packaging> Packaging { get; set; }
        public Shipping Shipping { get; set; }
        public int VariantId { get; set; }
        public List<Price7> Prices { get; set; }
        public string PriceIncludes { get; set; }
        public LowestPrice LowestPrice { get; set; }
        public HighestPrice HighestPrice { get; set; }
        public bool IsNew { get; set; }
        public bool IsConfirmed { get; set; }
        public bool HasFullColorProcess { get; set; }
        public bool HasRushService { get; set; }
        public string DistributorComments { get; set; }
        public string UpdateDate { get; set; }
    }

    public class Selections
    {
    }

    public class Dimensions
    {
        public string Length { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        
    }

    public class Links
    {
        public string Previous { get; set; }
        public string Self { get; set; }
        public string Next { get; set; }
    }

    public class ProductResult
    {
        public List<Result> Results { get; set; }
        public Selections Selections { get; set; }
        public Dimensions Dimensions { get; set; }
        public Links Links { get; set; }
        public string Breadcrumb { get; set; }
        public int Page { get; set; }
        public int ResultsPerPage { get; set; }
        public int ResultsTotal { get; set; }
        public int SuppliersTotal { get; set; }
        public double CompletedIn { get; set; }
    }

}
