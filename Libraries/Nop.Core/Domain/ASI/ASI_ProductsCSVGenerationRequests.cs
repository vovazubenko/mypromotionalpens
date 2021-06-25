using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.ASI
{
    public class ASI_ProductsCSVGenerationRequests : BaseEntity
    {
        public string GenerateFolderPath { get; set; }
        public ProductsCSVGenerationStatus Status { get; set; }
        public int SuccessfullyRetrivedProductsCount { get; set; }
        public double RecordsPerPage { get; set; }
        
        public bool ImageZipFileExists { get; set; }
        public bool ProductCSVExists { get; set; }

        public DateTime? AddedDate { get; set; }
        
        public DateTime? ModifiedDate { get; set; }
        public string IP { get; set; }

        private ICollection<ASI_ProductsSearchOptions> _SearchOptions;
        public virtual ICollection<ASI_ProductsSearchOptions> SearchOptions
        {
            get { return _SearchOptions ?? (_SearchOptions = new List<ASI_ProductsSearchOptions>()); }
             set { _SearchOptions = value; }
        }
    }

    public enum ProductsCSVGenerationStatus
    {
        Started,
        Running,
        Updating,
        WaitingToRun,
        Failed,
        Completed
    }
}
