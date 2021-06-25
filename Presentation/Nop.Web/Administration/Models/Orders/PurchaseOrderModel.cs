using FluentValidation.Attributes;
using Nop.Admin.Models.Vendors;
using Nop.Admin.Validators.Orders;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nop.Admin.Models.Orders
{
    [Validator(typeof(PurchaseOrderValidator))]
    public partial class PurchaseOrderModel : BaseNopEntityModel
    {
        public PurchaseOrderModel() {
            OrderModel = new OrderModel();
            VendorModel = new VendorModel();
            AvailableVendors = new List<SelectListItem>();
            Items = new List<PurchaseOrderItemModel>();
        }
        public Guid POGuid { get; set; }

        [NopResourceDisplayName("Admin.PurchaseOrder.Fields.PONumber")]
        public string PONumber { get; set; }

        [NopResourceDisplayName("Admin.PurchaseOrder.Fields.Ordernumber")]
        public int OrderId { get; set; }
        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.PurchaseOrder.Fields.PoDate")]
        public DateTime? PoDate { get; set; }
        [NopResourceDisplayName("Admin.PurchaseOrder.Fields.VendorId")]
        public int VendorId { get; set; }

        public IList<SelectListItem> AvailableVendors { get; set; }
        [NopResourceDisplayName("Admin.PurchaseOrder.Fields.VendorTitle")]
        public string VendorTitle { get; set; }
        [NopResourceDisplayName("Admin.PurchaseOrder.Fields.VendorAddress")]
        public string VendorAddress { get; set; }
        [NopResourceDisplayName("Admin.PurchaseOrder.Fields.VendorCity")]
        public string VendorCity { get; set; }
        [NopResourceDisplayName("Admin.PurchaseOrder.Fields.VendorState")]
        public string VendorState { get; set; }
        [NopResourceDisplayName("Admin.PurchaseOrder.Fields.VendorPostalCode")]
        public string VendorPostalCode { get; set; }
        [NopResourceDisplayName("Admin.PurchaseOrder.Fields.VendorEmail")]
        public string VendorEmail { get; set; }
        [NopResourceDisplayName("Admin.PurchaseOrder.Fields.POShipVia")]
        public string POShipVia { get; set; }
        [NopResourceDisplayName("Admin.PurchaseOrder.Fields.POTerm")]
        public string POTerm { get; set; }
        [NopResourceDisplayName("Admin.PurchaseOrder.Fields.PONotes")]
        [AllowHtml]
        public string PONotes { get; set; }
        [NopResourceDisplayName("Admin.PurchaseOrder.Fields.POAuthorizedBy")]
        public string POAuthorizedBy { get; set; }
        [NopResourceDisplayName("Admin.PurchaseOrder.Fields.POShippingCost")]
        public decimal? POShippingCost { get; set; }

        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.PurchaseOrder.Fields.PODeliveryDate")]
        public DateTime? PODeliveryDate { get; set; }

        public DateTime? POCreateDate { get; set; }

        public  OrderModel OrderModel { get; set; }
        public virtual VendorModel VendorModel { get; set; }

        public IList<PurchaseOrderItemModel> Items { get; set; }


        #region Nested Classes

        public partial class PurchaseOrderItemModel : BaseNopEntityModel
        {
            public PurchaseOrderItemModel()
            {
               
            }
            public int Po_id { get; set; }

            public string PONumber { get; set; }

            public string ProductCode { get; set; }

            public string ProductName { get; set; }

            public int? Quantity { get; set; }

            public decimal? Price { get; set; }

            public string UnitPrice{ get; set; }
            public int? QuantityReceived { get; set; }

            public decimal? ShippingCost { get; set; }

            public int? OrderDetailId { get; set; }

            public string AttributeDescription { get; set; }

            public string AttributesXml { get; set; }
            public decimal? SubtotalValue { get; set; }

            public string Subtotal { get; set; }



        }

    

     
 
   

        #endregion

    }
}