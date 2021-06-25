using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.SigmaSolve.Plugin.Redirects.Domain
{
    public class CustomRedirection : BaseEntity
    {
        public string Alias { get; set; }
        public string RedirectTo { get; set; }
        public bool PermanentRedirect { get; set; }
        public bool IsEnabled { get; set; }
    }
}
