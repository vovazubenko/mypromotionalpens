using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using Nop.Core;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Security;
using Nop.Services.Catalog;
using Nop.Services.Topics;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Filter;
using System.Web;
using Nop.Services.Logging;
using System.Data.SqlClient;

namespace Nop.Services.Seo
{
    /// <summary>
    /// Represents a sitemap generator
    /// </summary>
    public partial class SitemapGenerator : ISitemapGenerator
    {
        #region Constants

        private const string DateFormat = @"yyyy-MM-dd";

        /// <summary>
        /// At now each provided sitemap file must have no more than 50000 URLs
        /// </summary>
        private const int maxSitemapUrlNumber = 50000;//50000;

        #endregion

        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ITopicService _topicService;
        private readonly IWebHelper _webHelper;
        private readonly CommonSettings _commonSettings;
        private readonly BlogSettings _blogSettings;
        private readonly NewsSettings _newsSettings;
        private readonly ForumSettings _forumSettings;
        private readonly SecuritySettings _securitySettings;

        #endregion

        #region Ctor

        public SitemapGenerator(IStoreContext storeContext,
            ICategoryService categoryService,
            IProductService productService,
            IManufacturerService manufacturerService,
            ITopicService topicService,
            IWebHelper webHelper,
            CommonSettings commonSettings,
            BlogSettings blogSettings,
            NewsSettings newsSettings,
            ForumSettings forumSettings,
            SecuritySettings securitySettings)
        {
            this._storeContext = storeContext;
            this._categoryService = categoryService;
            this._productService = productService;
            this._manufacturerService = manufacturerService;
            this._topicService = topicService;
            this._webHelper = webHelper;
            this._commonSettings = commonSettings;
            this._blogSettings = blogSettings;
            this._newsSettings = newsSettings;
            this._forumSettings = forumSettings;
            this._securitySettings = securitySettings;
        }

        #endregion

        #region Nested class

        /// <summary>
        /// Represents sitemap URL entry
        /// </summary>
        protected class SitemapUrl
        {
            public SitemapUrl(string location, UpdateFrequency frequency, DateTime updatedOn)
            {
                Location = location;
                UpdateFrequency = frequency;
                UpdatedOn = updatedOn;
            }

            /// <summary>
            /// Gets or sets URL of the page
            /// </summary>
            public string Location { get; set; }

            /// <summary>
            /// Gets or sets a value indicating how frequently the page is likely to change
            /// </summary>
            public UpdateFrequency UpdateFrequency { get; set; }

            /// <summary>
            /// Gets or sets the date of last modification of the file
            /// </summary>
            public DateTime UpdatedOn { get; set; }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get HTTP protocol
        /// </summary>
        /// <returns>Protocol name as string</returns>
        protected virtual string GetHttpProtocol()
        {
            return _securitySettings.ForceSslForAllPages ? "https" : "http";
        }

        /// <summary>
        /// Generate URLs for the sitemap
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <returns>List of URL for the sitemap</returns>
        protected virtual IList<SitemapUrl> GenerateUrls(UrlHelper urlHelper)
        {
            var sitemapUrls = new List<SitemapUrl>();

            //
            string filterSiteMapPath = "";
            filterSiteMapPath = Path.Combine(HttpRuntime.AppDomainAppPath, "sitemap",
                String.Format("Filter.xml"));
            
            if (File.Exists(filterSiteMapPath))
            {
                var hostUrl = _webHelper.GetStoreLocation();
                filterSiteMapPath = hostUrl + String.Format("sitemap/Filter.xml");
                sitemapUrls.Add(new SitemapUrl(filterSiteMapPath, UpdateFrequency.Weekly, DateTime.UtcNow));
            }
            

            //home page
            var homePageUrl = urlHelper.RouteUrl("HomePage", null, GetHttpProtocol());
            sitemapUrls.Add(new SitemapUrl(homePageUrl, UpdateFrequency.Weekly, DateTime.UtcNow));

            //search products
            var productSearchUrl = urlHelper.RouteUrl("ProductSearch", null, GetHttpProtocol());
            sitemapUrls.Add(new SitemapUrl(productSearchUrl, UpdateFrequency.Weekly, DateTime.UtcNow));

            //contact us
            var contactUsUrl = urlHelper.RouteUrl("ContactUs", null, GetHttpProtocol());
            sitemapUrls.Add(new SitemapUrl(contactUsUrl, UpdateFrequency.Weekly, DateTime.UtcNow));

            //news
            if (_newsSettings.Enabled)
            {
                var url = urlHelper.RouteUrl("NewsArchive", null, GetHttpProtocol());
                sitemapUrls.Add(new SitemapUrl(url, UpdateFrequency.Weekly, DateTime.UtcNow));
            }

            //blog
            if (_blogSettings.Enabled)
            {
                var url = urlHelper.RouteUrl("Blog", null, GetHttpProtocol());
                sitemapUrls.Add(new SitemapUrl(url, UpdateFrequency.Weekly, DateTime.UtcNow));
            }

            //blog
            if (_forumSettings.ForumsEnabled)
            {
                var url = urlHelper.RouteUrl("Boards", null, GetHttpProtocol());
                sitemapUrls.Add(new SitemapUrl(url, UpdateFrequency.Weekly, DateTime.UtcNow));
            }

            //categories
            if (_commonSettings.SitemapIncludeCategories)
                sitemapUrls.AddRange(GetCategoryUrls(urlHelper, 0));

            //manufacturers
            if (_commonSettings.SitemapIncludeManufacturers)
                sitemapUrls.AddRange(GetManufacturerUrls(urlHelper));

            //products
            if (_commonSettings.SitemapIncludeProducts)
                sitemapUrls.AddRange(GetProductUrls(urlHelper));

            //topics
            sitemapUrls.AddRange(GetTopicUrls(urlHelper));

            //custom URLs
            sitemapUrls.AddRange(GetCustomUrls());

            //filter Urls

            //var _filterService = EngineContext.Current.Resolve<IFilterService>();
            //var filterMethod = _filterService.LoadFilterMethodBySystemName("FoxNetSoft.Plugin.Misc.SpeedFilters");

            //if (filterMethod != null)
            //{
            //    if (filterMethod.CategoryFilterEnabled())
            //    {
            //        var category = _categoryService.GetAllCategories().Where(x => x.Id == 953).ToList();
            //        foreach (var item in category)
            //        {
            //            var keyWord = _filterService.GetCustomKeyWord(item);
            //            sitemapUrls.AddRange(GetFilterCombinationUrl(keyWord, item.GetSeName(), item.Id));
            //        }


            //    }
            //}


            return sitemapUrls;
        }

        /// <summary>
        /// Get category URLs for the sitemap
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <param name="parentCategoryId">Parent category identifier</param>
        /// <returns>Collection of sitemap URLs</returns>
        protected virtual IEnumerable<SitemapUrl> GetCategoryUrls(UrlHelper urlHelper, int parentCategoryId)
        {
            return _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId).SelectMany(category =>
            {
                var sitemapUrls = new List<SitemapUrl>();
                var url = urlHelper.RouteUrl("Category", new { SeName = category.GetSeName() }, GetHttpProtocol());
                sitemapUrls.Add(new SitemapUrl(url, UpdateFrequency.Weekly, category.UpdatedOnUtc));
                sitemapUrls.AddRange(GetCategoryUrls(urlHelper, category.Id));

                return sitemapUrls;
            });
        }

        /// <summary>
        /// Get manufacturer URLs for the sitemap
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <returns>Collection of sitemap URLs</returns>
        protected virtual IEnumerable<SitemapUrl> GetManufacturerUrls(UrlHelper urlHelper)
        {
            return _manufacturerService.GetAllManufacturers(storeId: _storeContext.CurrentStore.Id).Select(manufacturer =>
            {
                var url = urlHelper.RouteUrl("Manufacturer", new { SeName = manufacturer.GetSeName() }, GetHttpProtocol());
                return new SitemapUrl(url, UpdateFrequency.Weekly, manufacturer.UpdatedOnUtc);
            });
        }

        /// <summary>
        /// Get product URLs for the sitemap
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <returns>Collection of sitemap URLs</returns>
        protected virtual IEnumerable<SitemapUrl> GetProductUrls(UrlHelper urlHelper)
        {
            return _productService.SearchProducts(storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true, orderBy: ProductSortingEnum.CreatedOn).Select(product =>
            {
                var url = urlHelper.RouteUrl("Product", new { SeName = product.GetSeName() }, GetHttpProtocol());
                return new SitemapUrl(url, UpdateFrequency.Weekly, product.UpdatedOnUtc);
            });
        }

        /// <summary>
        /// Get topic URLs for the sitemap
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <returns>Collection of sitemap URLs</returns>
        protected virtual IEnumerable<SitemapUrl> GetTopicUrls(UrlHelper urlHelper)
        {
            return _topicService.GetAllTopics(_storeContext.CurrentStore.Id).Where(t => t.IncludeInSitemap).Select(topic =>
            {
                var url = urlHelper.RouteUrl("Topic", new { SeName = topic.GetSeName() }, GetHttpProtocol());
                return new SitemapUrl(url, UpdateFrequency.Weekly, DateTime.UtcNow);
            });
        }

        /// <summary>
        /// Get custom URLs for the sitemap
        /// </summary>
        /// <returns>Collection of sitemap URLs</returns>
        protected virtual IEnumerable<SitemapUrl> GetCustomUrls()
        {
            var storeLocation = _webHelper.GetStoreLocation();

            return _commonSettings.SitemapCustomUrls.Select(customUrl =>
                new SitemapUrl(string.Concat(storeLocation, customUrl), UpdateFrequency.Weekly, DateTime.UtcNow));
        }

        /// <summary>
        /// Write sitemap index file into the stream
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <param name="stream">Stream</param>
        /// <param name="sitemapNumber">The number of sitemaps</param>
        protected virtual void WriteSitemapIndex(UrlHelper urlHelper, Stream stream, int sitemapNumber, List<List<SitemapUrl>> sitemaps, bool? GenerateFile = null)
        {
            if (GenerateFile == true)
            {
                //var filePath = Path.Combine(HttpRuntime.AppDomainAppPath, "Content\\files\\ExportImport", "Sitemap.xml");
                var filePath = Path.Combine(HttpRuntime.AppDomainAppPath, "sitemap", "categories.xml");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                var settings = new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8
                };

                using (stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (var writer = new XmlTextWriter(stream, Encoding.UTF8)/*XmlWriter.Create(stream, settings)*/)
                    {

                        //writer.Formatting = Formatting.Indented;
                        writer.WriteStartDocument();
                        writer.WriteStartElement("sitemapindex");
                        writer.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
                        writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                        writer.WriteAttributeString("xsi:schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");

                        for (var id = 1; id <= sitemapNumber; id++)
                        {
                            var siteUrl = sitemaps.ElementAt(id - 1);

                           var fileUrl= WriteSitemap(urlHelper, stream, siteUrl, true, id);

                            var url = urlHelper.RouteUrl("sitemap-indexed.xml", new { Id = id }, GetHttpProtocol());
                            if (!string.IsNullOrEmpty(fileUrl))
                                url = fileUrl;
                            var location = XmlHelper.XmlEncode(url);

                            writer.WriteStartElement("sitemap");
                            writer.WriteElementString("loc", location);
                            writer.WriteElementString("lastmod", DateTime.UtcNow.ToString(DateFormat));
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement(); // sitemapindex
                        writer.WriteEndDocument();
                    }
                }
            }
            else {
                using (var writer = new XmlTextWriter(stream, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartDocument();
                    writer.WriteStartElement("sitemapindex");
                    writer.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
                    writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    writer.WriteAttributeString("xsi:schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");

                    //write URLs of all available sitemaps
                    for (var id = 1; id <= sitemapNumber; id++)
                    {
                        var url = urlHelper.RouteUrl("sitemap-indexed.xml", new { Id = id }, GetHttpProtocol());
                        var location = XmlHelper.XmlEncode(url);

                        writer.WriteStartElement("sitemap");
                        writer.WriteElementString("loc", location);
                        writer.WriteElementString("lastmod", DateTime.UtcNow.ToString(DateFormat));
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
            }
        }

        /// <summary>
        /// Write sitemap file into the stream
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <param name="stream">Stream</param>
        /// <param name="sitemapUrls">List of sitemap URLs</param>
        protected virtual string WriteSitemap(UrlHelper urlHelper, Stream stream, IList<SitemapUrl> sitemapUrls, bool? GenerateFile = null,int? fileIndex=null)
        {
            if (GenerateFile == true) {

                var settings = new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8
                };

                //var filePath = Path.Combine(HttpRuntime.AppDomainAppPath, "content\\files\\exportimport",
                //    String.Format("Sitemap{0}.xml", fileIndex));
                var filePath = Path.Combine(HttpRuntime.AppDomainAppPath, "sitemap",
                    String.Format("categories{0}.xml", fileIndex));



                if (fileIndex == null || fileIndex == 0)
                {
                    filePath = Path.Combine(HttpRuntime.AppDomainAppPath, "sitemap",
                    String.Format("categories.xml", fileIndex));
                }
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                using (stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (var writer = new XmlTextWriter(stream, Encoding.UTF8)/* XmlWriter.Create(stream, settings)*/)
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");
                        writer.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
                        writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                        writer.WriteAttributeString("xsi:schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");


                        foreach (var url in sitemapUrls)
                        {
                            writer.WriteStartElement("url");
                            var location = XmlHelper.XmlEncode(url.Location);

                            writer.WriteElementString("loc", location);
                            writer.WriteElementString("changefreq", url.UpdateFrequency.ToString().ToLowerInvariant());
                            writer.WriteElementString("lastmod", url.UpdatedOn.ToString(DateFormat));
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement(); // urlset
                        writer.WriteEndDocument();
                    }
                }
                var hostUrl = _webHelper.GetStoreLocation();
                //filePath = hostUrl + String.Format("Sitemap{0}.xml", fileIndex);
                //filePath=urlHelper.RouteUrl("sitemap-indexed.xml", new { Id = fileIndex }, GetHttpProtocol());
                filePath = hostUrl + String.Format("sitemap/categories{0}.xml", fileIndex);
                return filePath;
            }
            else {

                using (var writer = new XmlTextWriter(stream, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartDocument();
                    writer.WriteStartElement("urlset");
                    writer.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
                    writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    writer.WriteAttributeString("xsi:schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");

                    //write URLs from list to the sitemap
                    foreach (var url in sitemapUrls)
                    {
                        writer.WriteStartElement("url");
                        var location = XmlHelper.XmlEncode(url.Location);

                        writer.WriteElementString("loc", location);
                        writer.WriteElementString("changefreq", url.UpdateFrequency.ToString().ToLowerInvariant());
                        writer.WriteElementString("lastmod", url.UpdatedOn.ToString(DateFormat));
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
            }
            return "";
        }

        #endregion

        #region Methods

        /// <summary>
        /// This will build an xml sitemap for better index with search engines.
        /// See http://en.wikipedia.org/wiki/Sitemaps for more information.
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <param name="id">Sitemap identifier</param>
        /// <returns>Sitemap.xml as string</returns>
        public virtual string Generate(UrlHelper urlHelper, int? id)
        {
            using (var stream = new MemoryStream())
            {
                Generate(urlHelper, stream, id);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// This will build an xml sitemap for better index with search engines.
        /// See http://en.wikipedia.org/wiki/Sitemaps for more information.
        /// </summary>
        /// <param name="urlHelper">URL helper</param>
        /// <param name="id">Sitemap identifier</param>
        /// <param name="stream">Stream of sitemap.</param>
        public virtual void Generate(UrlHelper urlHelper, Stream stream, int? id)
        {
            //generate all URLs for the sitemap
            var sitemapUrls = GenerateUrls(urlHelper);

            //split URLs into separate lists based on the max size 
            var sitemaps = sitemapUrls.Select((url, index) => new { Index = index, Value = url })
                .GroupBy(group => group.Index / maxSitemapUrlNumber).Select(group => group.Select(url => url.Value).ToList()).ToList();

            if (!sitemaps.Any())
                return;

            if (id.HasValue)
            {
                //requested sitemap does not exist
                if (id.Value == 0 || id.Value > sitemaps.Count)
                    return;

                //otherwise write a certain numbered sitemap file into the stream
                WriteSitemap(urlHelper, stream, sitemaps.ElementAt(id.Value - 1));

            }
            else
            {
                //URLs more than the maximum allowable, so generate a sitemap index file
                if (sitemapUrls.Count >= maxSitemapUrlNumber)
                {
                    //write a sitemap index file into the stream
                    WriteSitemapIndex(urlHelper, stream, sitemaps.Count, sitemaps, false);
                }
                else
                {
                    //otherwise generate a standard sitemap
                    WriteSitemap(urlHelper, stream, sitemaps.First(),false);
                }
            }
        }

        #endregion

        protected virtual IEnumerable<SitemapUrl> GetFilterCombinationUrl(string Keyword, string SeName, int CategoryId)
        {
            var _dbContext = EngineContext.Current.Resolve<IDbContext>();
            var storeLocation = _webHelper.GetStoreLocation();

            var KeywordParam = new SqlParameter
            {
                ParameterName = "@KeyWord",
                Value = Keyword
            };
            var SeNameParam = new SqlParameter
            {
                ParameterName = "@SeName",
                Value = SeName
            };
            var CategoryIdParam = new SqlParameter
            {
                ParameterName = "@CategoryId",
                Value = CategoryId
            };

            return _dbContext.SqlQuery<string>("EXEC GetFilterCombinationUrl @KeyWord,@SeName,@CategoryId", KeywordParam, SeNameParam, CategoryIdParam).ToList().Select(customUrl =>
                     new SitemapUrl(string.Concat(storeLocation, customUrl), UpdateFrequency.Weekly, DateTime.UtcNow));
        }

        #region Filter SiteMap
        public virtual string GenerateFilterUrl(UrlHelper urlHelper, int? id)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    GenerateFilterUrl(urlHelper, stream, id);
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
            catch (Exception ex) {
                var _logger = EngineContext.Current.Resolve<ILogger>();
                _logger.Error("Generate Filter sitemap error", ex);
                return "";
            }
        }
        public virtual void GenerateFilterUrl(UrlHelper urlHelper, Stream stream, int? id)
        {
            //generate all URLs for the sitemap
            var sitemapUrls = GetFilterUrls(urlHelper);

            //split URLs into separate lists based on the max size 
            var sitemaps = sitemapUrls.Select((url, index) => new { Index = index, Value = url })
                .GroupBy(group => group.Index / maxSitemapUrlNumber).Select(group => group.Select(url => url.Value).ToList()).ToList();

            if (!sitemaps.Any())
                return;

            if (id.HasValue)
            {
                //requested sitemap does not exist
                if (id.Value == 0 || id.Value > sitemaps.Count)
                    return;

                //otherwise write a certain numbered sitemap file into the stream
                WriteFilterSitemap(urlHelper, stream, sitemaps.ElementAt(id.Value - 1));

            }
            else
            {
                //URLs more than the maximum allowable, so generate a sitemap index file
                if (sitemapUrls.Count >= maxSitemapUrlNumber)
                {
                    //write a sitemap index file into the stream
                    WriteFilterSitemapIndex(urlHelper, stream, sitemaps.Count, sitemaps, true);
                }
                else
                {
                    //otherwise generate a standard sitemap
                    WriteFilterSitemap(urlHelper, stream, sitemaps.First(), true);
                }
            }
        }

        protected virtual IList<SitemapUrl> GetFilterUrls(UrlHelper urlHelper)
        {
            var sitemapUrls = new List<SitemapUrl>();
            //filter Urls

            var _filterService = EngineContext.Current.Resolve<IFilterService>();
            var filterMethod = _filterService.LoadFilterMethodBySystemName("FoxNetSoft.Plugin.Misc.SpeedFilters");
            var _logger = EngineContext.Current.Resolve<ILogger>();
            try
            {
                
                if (filterMethod != null)
                {
                    if (filterMethod.CategoryFilterEnabled())
                    {
                        var category = _categoryService.GetAllCategories().ToList();
                        var storeLocation = _webHelper.GetStoreLocation();
                        foreach (var item in category)
                        {
                            try {
                                var keyWord = _filterService.GetCustomKeyWord(item);
                                var catSename = "";
                                catSename = item.GetSeName();
                                //sitemapUrls.Add(new SitemapUrl(string.Concat(storeLocation, keyWord + "_" + catSename), UpdateFrequency.Weekly, DateTime.UtcNow));
                                sitemapUrls.AddRange(GetFilterCombinationUrl(keyWord, catSename, item.Id));
                                //sitemapUrls.AddRange(GetFilterCombinationUrl(keyWord, catSename, item.Id));
                            }
                            catch (Exception ex)
                            {
                                
                                _logger.Error("Generate Filter sitemap error for category=>"+item.Name, ex);
                            }
                        }


                    }
                }

            }
            catch (Exception ex) {
                _logger.Error("Generate Filter sitemap error", ex);
            }

            return sitemapUrls;
        }

        protected virtual string WriteFilterSitemap(UrlHelper urlHelper, Stream stream, IList<SitemapUrl> sitemapUrls, bool? GenerateFile = null, int? fileIndex = null)
        {
            if (GenerateFile == true)
            {

                var settings = new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8
                };

                var filePath = Path.Combine(HttpRuntime.AppDomainAppPath, "sitemap",
                    String.Format("Filter{0}.xml", fileIndex));



                if (fileIndex == null || fileIndex == 0)
                {
                    filePath = Path.Combine(HttpRuntime.AppDomainAppPath, "sitemap",
                    String.Format("Filter.xml", fileIndex));
                }
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                using (stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (var writer = new XmlTextWriter(stream, Encoding.UTF8)/* XmlWriter.Create(stream, settings)*/)
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");
                        writer.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
                        writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                        writer.WriteAttributeString("xsi:schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");


                        foreach (var url in sitemapUrls)
                        {
                            writer.WriteStartElement("url");
                            var location = XmlHelper.XmlEncode(url.Location);

                            writer.WriteElementString("loc", location);
                            writer.WriteElementString("changefreq", url.UpdateFrequency.ToString().ToLowerInvariant());
                            writer.WriteElementString("lastmod", url.UpdatedOn.ToString(DateFormat));
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement(); // urlset
                        writer.WriteEndDocument();
                    }
                }
                var hostUrl = _webHelper.GetStoreLocation();
                filePath = hostUrl + String.Format("sitemap/Filter{0}.xml", fileIndex);
                //filePath = urlHelper.RouteUrl("sitemap-indexed.xml", new { Id = fileIndex }, GetHttpProtocol());
                return filePath;
            }
            return "";
        }

        protected virtual void WriteFilterSitemapIndex(UrlHelper urlHelper, Stream stream, int sitemapNumber, List<List<SitemapUrl>> sitemaps, bool? GenerateFile = null)
        {
            if (GenerateFile == true)
            {
                var filePath = Path.Combine(HttpRuntime.AppDomainAppPath, "sitemap", "Filter.xml");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                var settings = new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8
                };

                using (stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (var writer = new XmlTextWriter(stream, Encoding.UTF8)/*XmlWriter.Create(stream, settings)*/)
                    {

                        //writer.Formatting = Formatting.Indented;
                        writer.WriteStartDocument();
                        writer.WriteStartElement("sitemapindex");
                        writer.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
                        writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                        writer.WriteAttributeString("xsi:schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");

                        for (var id = 1; id <= sitemapNumber; id++)
                        {
                            var siteUrl = sitemaps.ElementAt(id - 1);

                            var fileUrl = WriteFilterSitemap(urlHelper, stream, siteUrl, true, id);

                            var url = urlHelper.RouteUrl("sitemap-indexed.xml", new { Id = id }, GetHttpProtocol());
                            if (!string.IsNullOrEmpty(fileUrl))
                                url = fileUrl;
                            var location = XmlHelper.XmlEncode(url);

                            writer.WriteStartElement("sitemap");
                            writer.WriteElementString("loc", location);
                            writer.WriteElementString("lastmod", DateTime.UtcNow.ToString(DateFormat));
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement(); // sitemapindex
                        writer.WriteEndDocument();
                    }
                }
            }
        }
        #endregion
    }

}
