using Nop.Data.Mapping;
using Nop.SigmaSolve.Plugin.Redirects.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.SigmaSolve.Plugin.Redirects.Data
{
    public class CustomRedirection_Map : NopEntityTypeConfiguration<CustomRedirection>
    {
        public CustomRedirection_Map()
        {
            ToTable("CustomRedirect");
            HasKey(m => m.Id);
        }
    }
}
