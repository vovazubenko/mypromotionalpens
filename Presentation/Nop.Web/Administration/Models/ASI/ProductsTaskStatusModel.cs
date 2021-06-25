using Nop.Core;
using Nop.Core.Domain.ASI;
using Nop.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nop.Admin.Models.ASI
{
    public class ProductsTaskStatusModel
    {
        public List<ProductsSearchOptions> SearchOptions { get; set; }
        public bool TaskRunning { get; set; }

        public int SearchOptionCount { get; set; }
    }
    public class ProductsSearchOptions
    {
        public string Keyword { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public double TotalRecordsFound { get; set; }
        public double TotalRecordsRetrived { get; set; }
        public string TimeTaken { get; set; }

        public int Id { get; set; }

        public DateTime AddedDate { get; set; }


    }

    public class ASI_ProductsCSVGenerationRequestsModel
    {
        public List<ASI_ProductsCSVGenerationRequests> SearchRequests { get; set; }
        public bool TaskRunning { get; set; }

        public int SearchRequestsCount { get; set; }
    }

    public class ASIExistProductListModel
    {
        public int ASIProductId { get; set; }
        public int NopProductId { get; set; }
        public string ASIProductCode { get; set; }
        public string NopSku { get; set; }
        public string ASIProductManufacturer { get; set; }

        public decimal ASIProductPrice { get; set; }
        public decimal NopPrice { get; set; }

        public int requestId { get; set; }

        public int TotalRecords { get; set; }
    }
    public class ASIExistProductModel {

        [NopResourceDisplayName("Admin.ASI.ASIExistProduct.Fields.SearchName")]
        public string SearchName { get; set; }
        [NopResourceDisplayName("Admin.ASI.ASIExistProduct.Fields.Productcode")]
        public string Productcode { get; set; }


    }
}