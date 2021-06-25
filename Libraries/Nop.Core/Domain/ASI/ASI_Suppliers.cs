using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Core.Domain.ASI
{
    public class ASI_Suppliers : BaseEntity
    {
        public long SupplierId { get; set; }
        public string Name { get; set; }
        public long ASINumber { get; set; }
    }
}
