using iTextSharp.text;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Nop.Core;
using System.IO;
using Nop.Core.Domain.Common;

namespace Nop.Services.Orders
{
    public partial class PurchaseOrderService : IPurchaseOrderService
    {
        #region Fields
        private readonly IRepository<PurchaseOrder> _purchaseOrderRepository;
        private readonly IRepository<PurchaseOrderItem> _purchaseOrderItemRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly PdfSettings _pdfSettings;
        #endregion

        #region Ctor
        public PurchaseOrderService(IRepository<PurchaseOrder> purchaseOrderRepository,
            IEventPublisher eventPublisher,PdfSettings pdfSettings, IRepository<PurchaseOrderItem> purchaseOrderItemRepository
            )
        {
            this._purchaseOrderRepository = purchaseOrderRepository;
            this._eventPublisher = eventPublisher;
            this._pdfSettings = pdfSettings;
            this._purchaseOrderItemRepository = purchaseOrderItemRepository;

        }

        #endregion

        #region PurchaseOrder
        public virtual PurchaseOrder GetPOByOrderId(int orderid) {
            if (orderid == 0)
                return null;
            return _purchaseOrderRepository.Table.FirstOrDefault(o => o.OrderId == orderid);
        }

        public virtual PurchaseOrder GetPurchaseOrderById(int id)
        {
            if (id == 0)
                return null;
            return _purchaseOrderRepository.GetById(id);
        }

        public virtual void UpdatePurchaseOrder(PurchaseOrder po)
        {
            if (po == null)
                throw new ArgumentNullException("PurchaseOrder");

            //update
            _purchaseOrderRepository.Update(po);

            //event notification
            _eventPublisher.EntityUpdated(po);
        }

        public virtual void InsertPurchaseOrder(PurchaseOrder po)
        {
            if (po == null)
                throw new ArgumentNullException("PurchaseOrder");

            //insert
            _purchaseOrderRepository.Insert(po);

          
            //event notification
            _eventPublisher.EntityInserted(po);
        }

        public virtual void Deletepurchseorder(PurchaseOrder po) {
            if (po == null)
                throw new ArgumentNullException("PurchaseOrderItem");

            //delete
            _purchaseOrderRepository.Delete(po);

            //event notification
            _eventPublisher.EntityDeleted(po);
        }
        #endregion

        #region Purchase Order Item
        public virtual void InsertPurchaseOrderItem(PurchaseOrderItem poi) {
            if (poi == null)
                throw new ArgumentNullException("PurchaseOrder");

            //insert
            _purchaseOrderItemRepository.Insert(poi);

            //event notification
            _eventPublisher.EntityInserted(poi);

        }

        public virtual void UpdatePurchaseOrderItem(PurchaseOrderItem poi)
        {
            if (poi == null)
                throw new ArgumentNullException("PurchaseOrderItem");

            //update
            _purchaseOrderItemRepository.Update(poi);

            //event notification
            _eventPublisher.EntityUpdated(poi);
        }

        public virtual void DeletepurchseorderItem(PurchaseOrderItem poi)
        {
            if (poi == null)
                throw new ArgumentNullException("PurchaseOrderItem");

            //delete
            _purchaseOrderItemRepository.Delete(poi);

            //event notification
            _eventPublisher.EntityDeleted(poi);
        }

        public virtual PurchaseOrderItem GetPurchaseOrderItemById(int id)
        {
            if (id == 0)
                return null;
            return _purchaseOrderItemRepository.GetById(id);
        }
        #endregion
    }
}
