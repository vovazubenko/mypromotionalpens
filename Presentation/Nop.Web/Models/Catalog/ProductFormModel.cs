using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using FluentValidation.Attributes;
using Nop.Web.Validators.Common;

namespace Nop.Web.Models.Catalog
{
    [Validator(typeof(ProductRequestFormValidator))]
    public partial class ProductFormModel : BaseNopModel
    {
        public ProductFormModel()
        {
          
        }

        public int ProductId { get; set; }

        /// <summary>
        /// Query string
        /// </summary>
        [NopResourceDisplayName("Proofname.FullName")]
        
        public string FullName { get; set; }

        [NopResourceDisplayName("Proofname.Company")]
        
        public string Company { get; set; }

        [NopResourceDisplayName("Proofname.Address1")]
        
        public string Address1 { get; set; }

        [NopResourceDisplayName("Proofname.Address2")]
        
        public string Address2 { get; set; }

        [NopResourceDisplayName("Proofname.City")]
        
        public string City { get; set; }

        [NopResourceDisplayName("Proofname.State")]
        
        public string State { get; set; }

        [NopResourceDisplayName("Proofname.Zip")]
        
        public string Zip { get; set; }

        [NopResourceDisplayName("Proofname.EmailAddress")]
        
        public string EmailAddress { get; set; }

        [NopResourceDisplayName("Proofname.PhoneNumber")]
        
        public string PhoneNumber { get; set; }

        [NopResourceDisplayName("Proofname.ProductCode")]
        
        public string ProductCode { get; set; }

        [NopResourceDisplayName("Proofname.ProductColor")]
        
        public string ProductColor { get; set; }

        [NopResourceDisplayName("Proofname.ProductInfo")]
        [AllowHtml]
        public string ProductInfo { get; set; }


        [NopResourceDisplayName("Proofname.ProductCode1")]

        public string ProductCode1 { get; set; }

        [NopResourceDisplayName("Proofname.ProductCode2")]

        public string ProductCode2 { get; set; }

        [NopResourceDisplayName("Proofname.ProductCode3")]

        public string ProductCode3 { get; set; }

        [NopResourceDisplayName("Proofname.ProductCode4")]

        public string ProductCode4 { get; set; }


        [NopResourceDisplayName("Proofname.ProductCode5")]

        public string ProductCode5 { get; set; }

        [NopResourceDisplayName("Proofname.ProductColor1")]

        public string ProductColor1 { get; set; }


        [NopResourceDisplayName("Proofname.ProductColor2")]

        public string ProductColor2 { get; set; }

        [NopResourceDisplayName("Proofname.ProductColor3")]

        public string ProductColor3 { get; set; }


        [NopResourceDisplayName("Proofname.ProductColor4")]

        public string ProductColor4 { get; set; }

        [NopResourceDisplayName("Proofname.ProductColor5")]

        public string ProductColor5 { get; set; }


        [NopResourceDisplayName("Proofname.ProductImprintColor")]

        public string ProductImprintColor { get; set; }


        [NopResourceDisplayName("Proofname.ProductImprintColor2")]

        public string ProductImprintColor2 { get; set; }

        [NopResourceDisplayName("Proofname.ProductImprintColor3")]

        public string ProductImprintColor3 { get; set; }

        [NopResourceDisplayName("Proofname.ProductImprintColor4")]

        public string ProductImprintColor4 { get; set; }

        [NopResourceDisplayName("Proofname.ProductQty")]

        public string ProductQty { get; set; }

        [NopResourceDisplayName("Proofname.ProductQty2")]

        public string ProductQty2 { get; set; }

        [NopResourceDisplayName("Proofname.ProductQty3")]

        public string ProductQty3 { get; set; }

        [NopResourceDisplayName("Proofname.ProductQty4")]

        public string ProductQty4 { get; set; }

        [NopResourceDisplayName("Proofname.Country")]

        public string Country { get; set; }
        public string ProductName { get; set; }

        public string ItemColor { get; set; }
        public string ImprintColor { get; set; }
        public string ImprintText { get; set; }
        public string Comment { get; set; }
        public string ArtUpload { get; set; }

        public string Quantity { get; set; }
        public string ProductPictureUrl { get; internal set; }
    }
}