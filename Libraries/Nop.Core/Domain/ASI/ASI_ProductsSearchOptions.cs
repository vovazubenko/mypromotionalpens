using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nop.Core.Domain.ASI
{
    public class ASI_ProductsSearchOptions:BaseEntity
    {
        public int RequestId { get; set; }
        public ProductsSearchOptionType SearchOptionType { get; set; }
        public string SearchValue { get; set; }
        public virtual  ASI_ProductsCSVGenerationRequests Request { get; set; }
        public CurrentOptionStatus Status { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public double TotalRecords { get; set; }
        public DateTime AddedDate { get; set; }
        
        public DateTime ModifiedDate { get; set; }
        public string IP { get; set; }

        public int Retrivedproducts { get; set; }

    }
    public enum ProductsSearchOptionType
    {
        Supplier,
        Category
    }
    public enum CurrentOptionStatus
    {
        WaitingToRun,
        Running,
        Completed,
        Failed
    }

    public class SearchCategorySupplier {

        public SearchCategorySupplier() {
            AvailAbleSearchOptionType = new List<SelectListItem>();
        }
        public string SearchName { get; set;}
        public int csvBasedOn { get; set; }
        public IList<SelectListItem> AvailAbleSearchOptionType { get; set; }
    }
}
