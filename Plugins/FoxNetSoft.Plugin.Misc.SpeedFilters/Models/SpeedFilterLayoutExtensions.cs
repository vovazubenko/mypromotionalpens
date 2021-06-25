using Nop.Core.Domain.Seo;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters.Models
{
    public static class SpeedFilterLayoutExtensions
    {
        public static void AddMetaDescriptionParts(this HtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AddMetaDescriptionParts(part);
        }
    }

    public class SpeedFilterPageBuilder : PageHeadBuilder
    {
        private readonly List<string> _metaDescriptionParts;
        public SpeedFilterPageBuilder(SeoSettings seoSettings) :base(seoSettings)
        {
            this._metaDescriptionParts = new List<string>();
        }
        public override void AddMetaDescriptionParts(string part)
        {
            if (string.IsNullOrEmpty(part))
                return;

            _metaDescriptionParts.Add(part);
        }
    }
}
