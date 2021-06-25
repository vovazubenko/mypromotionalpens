using Nop.Web.Framework.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.SigmaSolve.Plugin.Redirects.Infrastructure
{
    public class CustomRedirectionViewEngine : ThemeableRazorViewEngine
    {
        public CustomRedirectionViewEngine()
        {
            PartialViewLocationFormats =
                new[]
                {
                    "~/Plugins/Nop.SigmaSolve.Plugin.Redirects/Views/{1}/{0}.cshtml"
                };

            ViewLocationFormats =
                new[]
                {
                    "~/Plugins/Nop.SigmaSolve.Plugin.Redirects/Views/{1}/{0}.cshtml"
                };
        }
    }
}
