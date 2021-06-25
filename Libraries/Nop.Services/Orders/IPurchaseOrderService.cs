using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Orders
{
    public partial interface IPurchaseOrderService
    {

        #region Purchase Order
        PurchaseOrder GetPOByOrderId(int orderid);

        PurchaseOrder GetPurchaseOrderById(int id);

        void UpdatePurchaseOrder(PurchaseOrder po);

        void InsertPurchaseOrder(PurchaseOrder po);

        void Deletepurchseorder(PurchaseOrder po);

        #endregion

        #region PurchaseOrderItem
        void InsertPurchaseOrderItem(PurchaseOrderItem poi);
        void UpdatePurchaseOrderItem(PurchaseOrderItem poi);
        void DeletepurchseorderItem(PurchaseOrderItem poi);

        PurchaseOrderItem GetPurchaseOrderItemById(int orderid);
        #endregion


    }
}
