using FoxNetSoft.Plugin.Misc.SpeedFilters.Domain;
using Nop.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters.Data
{
    public partial class SS_Specific_Category_Setting_Map: NopEntityTypeConfiguration<SS_Specific_Category_Setting>
    {
        public SS_Specific_Category_Setting_Map()
        {
            ToTable("SS_Specific_Category_Setting");
            HasKey(m => m.Id);
        }
    }
}
