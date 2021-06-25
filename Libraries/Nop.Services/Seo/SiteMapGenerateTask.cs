using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace Nop.Services.Seo
{
    public partial class SiteMapGenerateTask : ITask
    {
        #region Fields
        private readonly ISitemapGenerator _sitemapGenerator;
        #endregion

        #region ctor
        public SiteMapGenerateTask(ISitemapGenerator sitemapGenerator)
        {
            this._sitemapGenerator = sitemapGenerator;
        }
        #endregion

        #region Method
        public void Execute()
        {
            if (DateTime.Now.Hour == 2)

            {
                HttpContext.Current = CreateHttpContextCurrent();
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

                //_sitemapGenerator.Generate(urlHelper, null);
                _sitemapGenerator.GenerateFilterUrl(urlHelper, null);
            }
        }

        private HttpContext CreateHttpContextCurrent()
        {
            var _webHelper = EngineContext.Current.Resolve<IWebHelper>();
            var storeLocation = _webHelper.GetStoreLocation();
            var httpRequest = new HttpRequest(string.Empty,storeLocation, string.Empty);
            var stringWriter = new StringWriter();
            var httpResponce = new HttpResponse(stringWriter);
            var httpContext = new HttpContext(httpRequest, httpResponce);

            return httpContext;
        }
        #endregion
    }
}
