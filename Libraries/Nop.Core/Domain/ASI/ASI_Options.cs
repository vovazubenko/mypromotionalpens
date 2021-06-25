using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.ASI
{
    public partial class ASI_Options : BaseEntity
    {
        public long? OptionCatId { get; set; }

        public string OptionsName { get; set; }

        public string OptionsDesc { get; set; }

        public string ProductCode { get; set; }

        public DateTime? AddedDate { get; set; }
        
        public DateTime? ModifiedDate { get; set; }
        public string IP { get; set; }
    }
}
