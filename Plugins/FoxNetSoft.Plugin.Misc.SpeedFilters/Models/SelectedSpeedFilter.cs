using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Common;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Logger;
using System.Linq;
using Nop.Services.Catalog;
using Nop.Core.Infrastructure;
using Nop.Core.Data;
using Nop.Data;
using System.Data.Entity.Core.Objects;
using Nop.Services.Filter;
using FoxNetSoft.Plugin.Misc.SpeedFilters.Services;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters.Models
{
    /// <summary>
    /// Represents a SelectedSpeedFilter
    /// </summary>
    public class SelectedSpeedFilter
    {
        #region Const

        private const string QUERYSTRINGPARAM = "price";

        private const string SPEEDFILTER_KEYWORD = "[Keyword]";
        private const string SPEEDFILTER_CATEGORY = "[Category Name]";
        private const string SPEEDFILTER_PRODUCTIONTIME = "[Production Time]";
        private const string SPEEDFILTER_INKCOLOR = "[Ink Color]";
        private const string SPEEDFILTER_COLOR = "[Color]";
        private const string SPEEDFILTER_MATERIAL = "[Material]";
        #endregion

        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly SpeedFiltersSettings _speedFiltersSettings;
        private readonly ISpeedFiltersService _speedFiltersService;
        private readonly FNSLogger _fnsLogger;
        private readonly IRepository<SpecificationAttribute> _specificationAttributeRepository;
        private readonly IRepository<SpecificationAttributeOption> _specificationAttributeOptionRepository;
        private readonly IDbContext _context;

        private static Dictionary<string, string> _metaReplacements = new Dictionary<string, string>();

        #endregion

        #region Constructors

        public SelectedSpeedFilter(
            CatalogSettings catalogSettings,
            SpeedFiltersSettings speedFiltersSettings,
            FNSLogger fnsLogger)
        {
            this._catalogSettings = catalogSettings;
            this._speedFiltersSettings = speedFiltersSettings;
            this._fnsLogger = fnsLogger;
            this._specificationAttributeRepository = EngineContext.Current.Resolve<IRepository<SpecificationAttribute>>();
            this._specificationAttributeOptionRepository = EngineContext.Current.Resolve<IRepository<SpecificationAttributeOption>>();
            this._context = EngineContext.Current.Resolve<IDbContext>();
            this._speedFiltersService = EngineContext.Current.Resolve<ISpeedFiltersService>();
            OrderBy = (int)this._speedFiltersSettings.DefaultProductSorting;
            ViewMode = _catalogSettings.DefaultViewMode;
            priceRange = new PriceRange() { From = null, To = null };
            PageSize = 0;
            PageNumber = 0;
            ShowOnSaldo = "all";

            manufacturerIds = new List<int>();
            vendorIds = new List<int>();
            searchModel = new FoxNetSoft.Plugin.Misc.SpeedFilters.Domain.SearchModel();
            specFilterIds = new List<SelectedSpeedFilter.FilterElement>();
            attrFilterIds = new List<SelectedSpeedFilter.FilterElement>();

            Products = new List<ProductOverviewModel>();
            PagingFilteringContext = new CatalogPagingFilteringModel();
            pagerModel = new PagerModel();

            _metaReplacements[SPEEDFILTER_KEYWORD] = "";
            _metaReplacements[SPEEDFILTER_CATEGORY] = "";
            _metaReplacements[SPEEDFILTER_PRODUCTIONTIME] = "";
            _metaReplacements[SPEEDFILTER_INKCOLOR] = "";
            _metaReplacements[SPEEDFILTER_COLOR] = "";
            _metaReplacements[SPEEDFILTER_MATERIAL] = "";
        }
        #endregion

        #region Properties

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string ViewMode { get; set; }
        public int OrderBy { get; set; }
        public string ShowOnSaldo { get; set; }

        public PriceRange priceRange { get; set; }

        public int categoryId { get; set; }

        /// Manufacturer  
        public IList<int> manufacturerIds { get; set; }

        /// Vendor  
        public IList<int> vendorIds { get; set; }

        //SearchPage
        public FoxNetSoft.Plugin.Misc.SpeedFilters.Domain.SearchModel searchModel { get; set; }

        //SpecificationsFilter
        public IList<SelectedSpeedFilter.FilterElement> specFilterIds { get; set; }

        //ProductAttribureFilter
        public IList<SelectedSpeedFilter.FilterElement> attrFilterIds { get; set; }

        public IList<int> filterableSpecificationAttributeOptionIds { get; set; }
        public IList<int> filterableProductAttributeOptionIds { get; set; }
        public IList<int> filterableManufacturerIds { get; set; }
        public IList<int> filterableVendorIds { get; set; }


        public CatalogPagingFilteringModel PagingFilteringContext { get; set; }
        public IList<ProductOverviewModel> Products { get; set; }

        public PagerModel pagerModel { get; set; }

        public bool hasError { get; set; }
        public string ErrorMessage { get; set; }

        public bool showEmptyResult { get; set; }

        #endregion

        #region Utils

        private void LogMessage(string message)
        {
            if (this._fnsLogger != null)
            {
                //this.showDebugInfo
                this._fnsLogger.LogMessage(message);
            }
        }


        /// <summary>
        /// Gets query string value by name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newurl">New url</param>
        /// <param name="name">Parameter name</param>
        /// <returns>Query string value</returns>
        public T QueryString<T>(string newurl, string name)
        {
            /*
            --?viewmode=list
            --?viewmode=grid             
             */
            string queryParam = null;
            string searchstr = name.Trim().ToLower() + "=";
            int charparam = newurl.Trim().ToLower().IndexOf(searchstr);
            if (charparam != -1)
            {
                queryParam = newurl.Substring(charparam + searchstr.Length);
                charparam = queryParam.IndexOf("&");
                if (charparam != -1)
                    queryParam = queryParam.Substring(0, charparam);
            }
            if (!String.IsNullOrEmpty(queryParam))
                return CommonHelper.To<T>(queryParam);
            return default(T);
        }

        private PriceRange GetSelectedPriceRange(string newurl)
        {
            //old format &price=1000-1200
            //price=785
            string range = QueryString<string>(newurl, QUERYSTRINGPARAM);
            decimal? from = null;
            decimal? to = null;

            if (!String.IsNullOrEmpty(range))
            {
                string[] fromTo = range.Trim().Split(new char[] { '-' });
                if (fromTo.Length == 1)
                {
                    if (!String.IsNullOrEmpty(fromTo[0]) && !String.IsNullOrEmpty(fromTo[0].Trim()))
                        from = decimal.Parse(fromTo[0].Trim(), new CultureInfo("en-US"));
                }
                if (fromTo.Length == 2)
                {
                    if (!String.IsNullOrEmpty(fromTo[0]) && !String.IsNullOrEmpty(fromTo[0].Trim()))
                        from = decimal.Parse(fromTo[0].Trim(), new CultureInfo("en-US"));
                    if (!String.IsNullOrEmpty(fromTo[1]) && !String.IsNullOrEmpty(fromTo[1].Trim()))
                        to = decimal.Parse(fromTo[1].Trim(), new CultureInfo("en-US"));

                    //return new PriceRange() { From = from, To = to };
                }
            }

            return new PriceRange() { From = from, To = to };

            /*
            //new format
            //&prFilter=From-0.12To-120.00
            range = QueryString<string>(newurl, "prFilter=");
            if (!String.IsNullOrEmpty(range))
            {
                //From-729To-1132 last block
                decimal? from = null;
                decimal? to = null;

                int posChar = 0;
                posChar = range.IndexOf("From-");
                if (posChar != 1)
                {
                    decimal parcevalue = 0;
                    int posChar2 = range.IndexOf("To-");
                    //from = decimal.Parse(range.Substring(posChar + 5, posChar2 - posChar-5).Trim(), new CultureInfo("en-US"));
                    string range2;
                    if (posChar2==-1)
                        range2 = range.Substring(posChar + 5);
                    else
                        range2=range.Substring(posChar + 5, posChar2 - posChar - 5);
                    if (decimal.TryParse(range2.Trim(), out parcevalue))
                    {
                        from=parcevalue;
                    }
                    else
                    {
                        from = null;
                    }
                }
                posChar = range.IndexOf("To-");
                if (posChar != 1)
                {
                    decimal parcevalue = 0;
                    int posChar2 = range.IndexOf("!");
                    //from = decimal.Parse(range.Substring(posChar + 5, posChar2 - posChar-5).Trim(), new CultureInfo("en-US"));
                    string range2;
                    if (posChar2 == -1)
                        range2 = range.Substring(posChar + 5);
                    else
                        range2 = range.Substring(posChar + 5, posChar2 - posChar - 5);
                    if (decimal.TryParse(range2.Trim(), out parcevalue))
                    {
                        to = parcevalue;
                    }
                    else
                    {
                        to = null;
                    }
                }

                return new PriceRange() { From = from, To = to };
            }*/

        }

        private IList<int> GetSelectedManufacturers(string newurl, string name)
        {
            IList<int> sellist = new List<int>();
            newurl = newurl.ToLower().Trim();

            //  manFilters=1,2,3,4,5,6&
            //  manFilters=1,2,3,4,5,6
            string selectedlist = QueryString<string>(newurl, name.ToLower());

            if (!String.IsNullOrEmpty(selectedlist))
            {
                string[] strIds = selectedlist.Trim().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var strId in strIds)
                {
                    if (!String.IsNullOrWhiteSpace(strId))
                    {
                        int id = 0;
                        if (int.TryParse(strId.Trim(), out id))
                        {
                            if (id > 0)
                                sellist.Add(id);
                        }
                    }
                }
            }

            return sellist;
        }

        private IList<SelectedSpeedFilter.FilterElement> GetSelectedSpecFilters(string filerstring, string name)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("SelectedSpeedFilter. GetSelectedSpecFilters. name={0}, filerstring={1}", name, filerstring));
            IList<SelectedSpeedFilter.FilterElement> sellist = new List<SelectedSpeedFilter.FilterElement>();
            filerstring = filerstring.ToLower().Trim();
            //specFilters=25!1,2,3,4;18!25,2,69,56&
            //specFilters=25!1,2,3,4;18!25,2,69,56
            //attrFilters=4!14


            string selectedlist = QueryString<string>(filerstring, name.ToLower());
            sb.AppendLine(String.Format("                 selectedlist={0}", selectedlist));

            if (!String.IsNullOrEmpty(selectedlist))
            {
                //25!1,2,3,4;18=25,2,69,56
                int blockId;
                string[] strGroupIds = selectedlist.Trim().Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var strGroupId in strGroupIds)
                {
                    sb.AppendLine(String.Format("                 strGroupId={0}", strGroupId));

                    //25!1,2,3,4
                    //18!25,2,69,56
                    var posGroupMark = strGroupId.IndexOf('!');
                    if (posGroupMark != -1)
                    {
                        //sb.Append(String.Format("    posGroupMark={0}", posGroupMark));
                        if (int.TryParse(strGroupId.Substring(0, posGroupMark), out blockId))
                        {
                            var strGroupId2 = strGroupId.Substring(posGroupMark + 1);
                            //sb.Append(String.Format(", strGroupId2={0}", strGroupId2));
                            string[] strIds = strGroupId2.Trim().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var strId in strIds)
                            {
                                //sb.Append(String.Format(", strId={0}", strId));
                                if (!String.IsNullOrWhiteSpace(strId))
                                {
                                    int id = 0;
                                    if (int.TryParse(strId.Trim(), out id))
                                    {
                                        //sb.Append(String.Format(", id={0}", id));
                                        if (id > 0)
                                            sellist.Add(new SelectedSpeedFilter.FilterElement()
                                            {
                                                BlockId = blockId,
                                                Id = id
                                            });
                                        sb.AppendLine(String.Format("                          blockId={0}, id={1}", blockId, id));
                                    }
                                }
                            }
                        }
                    }
                    sb.AppendLine("");
                }
            }
            LogMessage(sb.ToString());
            return sellist;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parce parameters from url
        /// </summary>
        /// <param name="filerstring">New url</param>
        public virtual SpeedFilterSelectedAttr GetParameters(string filerstring)
        {
            this.showEmptyResult = false;
            string strNameUrl = "";
            bool allowUrlGenerate = true;
            Dictionary<string, string> dictSpecificationAttr = new Dictionary<string, string>();
            //specFilters=25=1,2,3;2=1,7,8,9&attrFilters=25=1,2,3;2=1,7,8,9&manFilters=1,2,3,4&venFilters=1,2,3,4&price=1000-1200

            //http://mebli.ars.ua/dom-mahkaja-mebel-komplekty#/specFilters=25!#-!83&pageSize=45&viewMode=grid&orderBy=0&pageNumber=1
            //http://demo320.foxnetsoft.com/desktops?pagesize=12&orderby=6&viewmode=list
            //http://mebli.ars.ua/dom-mahkaja-mebel-komplekty?pagesize=12&orderby=6&viewmode=list#/specFilters=25!#-!83&pageSize=45&viewMode=grid&orderBy=0&pageNumber=1
            //http://mebli.ars.ua/dom-mahkaja-mebel-komplekty#/specFilters=25!#-!82&pageSize=45&viewMode=grid&orderBy=0&pageNumber=1
            //http://demo320.foxnetsoft.com/desktops?viewmode=list&pagesize=12

            //sorting
            //&orderby=6
            var orderby = QueryString<string>(filerstring, "orderby");
            if (String.IsNullOrWhiteSpace(orderby))
                OrderBy = (int)this._speedFiltersSettings.DefaultProductSorting;
            else
            {
                int orderbyint = 0;
                int.TryParse(orderby, out orderbyint);
                this.OrderBy = orderbyint;
            }

            //(int)this._speedFiltersSettings.DefaultProductSorting; 

            //view mode
            //--?viewmode=list
            //--?viewmode=grid
            ViewMode = QueryString<string>(filerstring, "viewmode");
            if (String.IsNullOrWhiteSpace(ViewMode))
                ViewMode = _catalogSettings.DefaultViewMode;
            //pageSize
            PageSize = QueryString<int>(filerstring, "pageSize");

            //PageNumber
            PageNumber = QueryString<int>(filerstring, "pageNumber");

            //showOnSaldo
            ShowOnSaldo = QueryString<string>(filerstring, "viewzal");
            if (String.IsNullOrWhiteSpace(ShowOnSaldo))
                ShowOnSaldo = "all";
            if (ShowOnSaldo != "all")
                this.showEmptyResult = true;

            //PriceRange
            priceRange = GetSelectedPriceRange(filerstring);
            if (priceRange.From != null || priceRange.To != null)
            {
                this.showEmptyResult = true;
                allowUrlGenerate = false;
            }

            //Manufacturers  manFilters
            manufacturerIds = GetSelectedManufacturers(filerstring, "mFilters");
            if (manufacturerIds.Count > 0)
            {
                allowUrlGenerate = false;
                this.showEmptyResult = true;
            }

            //Vendors      venFilters
            vendorIds = GetSelectedManufacturers(filerstring, "vFilters");
            if (vendorIds.Count > 0)
            {
                allowUrlGenerate = false;
                this.showEmptyResult = true;
            }

            //specFilters
            specFilterIds = GetSelectedSpecFilters(filerstring, "sFilters");
            if (specFilterIds.Count > 0)
                this.showEmptyResult = true;

            //attrFilters
            attrFilterIds = GetSelectedSpecFilters(filerstring, "aFilters");
            if (attrFilterIds.Count > 0)
            {
                allowUrlGenerate = false;
                this.showEmptyResult = true;
            }

            //search page

            var searchpagestr = string.Empty;
            //QueryStringForSeacrh
            var searchTerms = QueryString<string>(filerstring, "q");

            if (string.IsNullOrWhiteSpace(searchTerms))
                searchModel.QueryStringForSeacrh = "";
            else
                searchModel.QueryStringForSeacrh = searchTerms.Trim();

            if (!string.IsNullOrWhiteSpace(searchModel.QueryStringForSeacrh))
                searchModel.Enabled = true;

            //Advanced search
            searchModel.AdvancedSearch = QueryString<bool>(filerstring, "adv");

            if (searchModel.AdvancedSearch)
            {
                searchModel.Enabled = true;
                //Include SubCategories
                searchModel.IncludeSubCategories = QueryString<bool>(filerstring, "isc");

                //Search In Descriptions
                searchModel.SearchInDescriptions = QueryString<bool>(filerstring, "sid");

                //category Id
                searchpagestr = QueryString<string>(filerstring, "cid");
                int cid;
                if (!string.IsNullOrWhiteSpace(searchpagestr) && int.TryParse(searchpagestr, out cid))
                    this.categoryId = cid;

                //manufacturer Id
                searchpagestr = QueryString<string>(filerstring, "mid");
                int mid;
                if (!string.IsNullOrWhiteSpace(searchpagestr) && int.TryParse(searchpagestr, out mid))
                {
                    if (!manufacturerIds.Contains(mid))
                        manufacturerIds.Add(mid);
                }

                //price from
                searchpagestr = QueryString<string>(filerstring, "pf");
                decimal pf;
                if (!string.IsNullOrWhiteSpace(searchpagestr) && decimal.TryParse(searchpagestr, out pf))
                {
                    if (!priceRange.From.HasValue || priceRange.From < pf)
                        priceRange.From = pf;
                }

                //price to
                searchpagestr = QueryString<string>(filerstring, "pt");
                decimal pt;
                if (!string.IsNullOrWhiteSpace(searchpagestr) && decimal.TryParse(searchpagestr, out pt))
                {
                    if (!priceRange.From.HasValue || priceRange.From > pt)
                        priceRange.From = pt;
                }
            }
            if(!string.IsNullOrEmpty(searchpagestr))
            {
                allowUrlGenerate = false;
            }
            #region url generate
            {
                if (allowUrlGenerate && specFilterIds.Any())
                {
                    var myspecAttrString = new string[] { "production time", "ink color", "color", "material" }; // chnage from settings
                    var mySpecAttrIds = _specificationAttributeRepository.Table
                        .Where(x => myspecAttrString.Contains(x.Name.ToLower()))
                        .OrderByDescending(x => x.DisplayOrder).Select(x => x.Id).ToList();
                    List<int> specificationAttributeIds = specFilterIds.GroupBy(x => x.BlockId)
                        .Select(r => r.First().BlockId).ToList();

                    List<int> allFltrSpecOptionIds = specFilterIds.Select(x => x.Id).ToList();
                    List<SpecificationAttributeOption> allFltrSpecOptions = _specificationAttributeOptionRepository.Table.
                                       Where(x => allFltrSpecOptionIds.Contains(x.Id)).ToList();

                    if (!specificationAttributeIds.Any(row => !mySpecAttrIds.Contains(row)))
                    {
                        var count = -1;
                        foreach (var item in mySpecAttrIds)
                        {
                            string fltrSpecOptionUrl = "";
                            string fltrSpecOptionName = "";
                            count++;
                            if (count > 0)
                            {
                                strNameUrl = strNameUrl + "_";
                            }
                            if (!specificationAttributeIds.Contains(item))
                            {
                                //if (count>=2)
                                //{
                                //    strNameUrl = strNameUrl + "_";
                                //}

                            }
                            else {
                                List<int> fltrSpecOptionIds = specFilterIds.Where(x => x.BlockId == item).Select(x => x.Id).ToList();
                                
                                if (fltrSpecOptionIds.Any())
                                {
                                    //List<string> fltrSpecOptionNames = _specificationAttributeOptionRepository.Table.
                                    //    Where(x => fltrSpecOptionIds.Contains(x.Id)).OrderBy(x => x.Name).Select(x => x.Name).ToList();
                                    List<string> fltrSpecOptionNames = allFltrSpecOptions.Where(x => x.SpecificationAttributeId == item)
                                        .OrderBy(x => x.Name).Select(x => x.Name).ToList();
                                    if (fltrSpecOptionNames.Any())
                                    {
                                        foreach (var strItem in fltrSpecOptionNames)
                                        {

                                            //string result = _context.SqlQuery<string>(
                                            //"SELECT dbo.GetURLSlug('" + strItem + "','false')").First();
                                            string result = GetSeName(strItem, true);
                                            fltrSpecOptionUrl = string.IsNullOrEmpty(fltrSpecOptionUrl) ? result : fltrSpecOptionUrl + "~" + result;

                                            fltrSpecOptionName = string.IsNullOrEmpty(fltrSpecOptionName) ? strItem : fltrSpecOptionName + " " + strItem;
                                        }
                                    }
                                }
                                //strNameUrl = string.IsNullOrEmpty(strNameUrl) ? fltrSpecOptionUrl : strNameUrl + "_" + fltrSpecOptionUrl;
                                strNameUrl = string.IsNullOrEmpty(strNameUrl) ? fltrSpecOptionUrl : strNameUrl +  fltrSpecOptionUrl;
                            }

                            dictSpecificationAttr.Add(myspecAttrString[count], fltrSpecOptionName);
                        }


                    }

                }
            }
            #endregion

            SpeedFilterSelectedAttr speedFilterSelectedAttr = new SpeedFilterSelectedAttr();
            speedFilterSelectedAttr.CompositeUrl = strNameUrl;
            speedFilterSelectedAttr.dictSpecAttr = dictSpecificationAttr;
            //return strNameUrl;
            return speedFilterSelectedAttr;
        }
        public string GetSeName(string name, bool allowUnicodeCharsInUrls)
        {
            if (String.IsNullOrEmpty(name))
                return name;
            //string okChars = "abcdefghijklmnopqrstuvwxyz1234567890 _-";
            string okChars = "abcdefghijklmnopqrstuvwxyz1234567890 -";
            name = name.Trim().ToLowerInvariant();


            var sb = new StringBuilder();
            foreach (char c in name.ToCharArray())
            {
                string c2 = c.ToString();


                if (allowUnicodeCharsInUrls)
                {
                    if (char.IsLetterOrDigit(c) || okChars.Contains(c2))
                        sb.Append(c2);
                }
                else if (okChars.Contains(c2))
                {
                    sb.Append(c2);
                }
            }
            string name2 = sb.ToString();
            name2 = name2.Replace(" ", "-");
            while (name2.Contains("--"))
                name2 = name2.Replace("--", "-");
            //while (name2.Contains("__"))
            //    name2 = name2.Replace("__", "_");
            while (name2.Contains("__"))
                name2 = name2.Replace("__", "-");
            return name2;
        }

        public SpeedFilterSeoModel PrepareMetaTags(SpeedFilterSelectedAttr data, Category category) {
            SpeedFilterSeoModel model = new SpeedFilterSeoModel();
            //List<int> speciFiedCategories = new List<int>();
            //if (!string.IsNullOrEmpty(_speedFiltersSettings.OptimizedCategory))
            //    speciFiedCategories = _speedFiltersSettings.OptimizedCategory.Split(',').Select(x => Convert.ToInt32(x)).ToList();

            var specificCategorySetting = _speedFiltersService.GetSpecificCategorySettingByCategoryId(category.Id);

            _metaReplacements[SPEEDFILTER_KEYWORD] = "";
            _metaReplacements[SPEEDFILTER_CATEGORY] = category.Name;     
            if (data != null && data.dictSpecAttr != null)
            {
                _metaReplacements[SPEEDFILTER_PRODUCTIONTIME] =data.dictSpecAttr.ContainsKey("production time")? data.dictSpecAttr["production time"]:string.Empty;
                _metaReplacements[SPEEDFILTER_INKCOLOR] = data.dictSpecAttr.ContainsKey("ink color") ? data.dictSpecAttr["ink color"] : string.Empty;
                _metaReplacements[SPEEDFILTER_COLOR] = data.dictSpecAttr.ContainsKey("color") ? data.dictSpecAttr["color"] : string.Empty;
                _metaReplacements[SPEEDFILTER_MATERIAL] = data.dictSpecAttr.ContainsKey("material") ? data.dictSpecAttr["material"] : string.Empty;
            }
            

            if (specificCategorySetting != null)
            {
                // get specified meta tags
                _metaReplacements[SPEEDFILTER_KEYWORD] = specificCategorySetting.CustomKeyword;

                model.HeaderCopy = specificCategorySetting.HeaderCopy;
                model.HeaderTitle = specificCategorySetting.HeaderTitle;
                model.HTag = specificCategorySetting.H1Tag;
                model.H2Tag = specificCategorySetting.H2Tag;
                model.KeyWord= specificCategorySetting.CustomKeyword;
                model.MetaDescription = specificCategorySetting.MetaDescription;
                model.MetaKeyWord = specificCategorySetting.MetaKeyword;
                model.MetaTitle = specificCategorySetting.MetaTitle;
                model.FooterContent1 = specificCategorySetting.FooterContent1;
                model.FooterContent2 = specificCategorySetting.FooterContent2;
                model.FooterContent3 = specificCategorySetting.FooterContent3;
                model.FooterTitle1 = specificCategorySetting.FooterTitle1;
                model.FooterTitle2 = specificCategorySetting.FooterTitle2;
                model.FooterTitle3 = specificCategorySetting.FooterTitle3;
            }
            else {
                _metaReplacements[SPEEDFILTER_KEYWORD] =_speedFiltersSettings.GlobalCustomKeyword;
                // general meta tags
                model.HeaderCopy = _speedFiltersSettings.GlobalHeaderCopy;
                model.HeaderTitle = _speedFiltersSettings.GlobalHeaderTitle;
                model.HTag = _speedFiltersSettings.GlobalHTag;
                model.KeyWord = _speedFiltersSettings.GlobalCustomKeyword;
                model.MetaDescription = _speedFiltersSettings.GlobalMetaDescription;
                model.MetaKeyWord = _speedFiltersSettings.GlobalMetaKeyWord;
                model.MetaTitle = _speedFiltersSettings.GlobalMetaTitle;
                model.FooterContent1 = _speedFiltersSettings.GlobalFooterContent1;
                model.FooterContent2 = _speedFiltersSettings.GlobalFooterContent2;
                model.FooterContent3 = _speedFiltersSettings.GlobalFooterContent3;
                model.FooterTitle1 = _speedFiltersSettings.GlobalFooterTitle1;
                model.FooterTitle2 = _speedFiltersSettings.GlobalFooterTitle2;
                model.FooterTitle3 = _speedFiltersSettings.GlobalFooterTitle3;

            }
            foreach (string to_replace in _metaReplacements.Keys)
            {
                model.HeaderCopy =string.IsNullOrEmpty(model.HeaderCopy)?"":model.HeaderCopy
                    .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace])?""+to_replace: to_replace, _metaReplacements[to_replace]);

                model.HeaderTitle  = string.IsNullOrEmpty(model.HeaderTitle) ? "" : model.HeaderTitle
                    .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace]) ? "" + to_replace : to_replace, _metaReplacements[to_replace]);

                model.HTag = string.IsNullOrEmpty(model.HTag) ? "" : model.HTag
                    .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace]) ? " " + to_replace : to_replace, _metaReplacements[to_replace])
                    .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace]) ? "" + to_replace : to_replace, _metaReplacements[to_replace]);

                model.H2Tag = string.IsNullOrEmpty(model.H2Tag) ? "" : model.H2Tag
                    .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace]) ? " " + to_replace : to_replace, _metaReplacements[to_replace])
                    .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace]) ? "" + to_replace : to_replace, _metaReplacements[to_replace]);

                model.KeyWord = string.IsNullOrEmpty(model.KeyWord) ? "" : model.KeyWord
                    .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace]) ? " " + to_replace : to_replace, _metaReplacements[to_replace])
                    .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace]) ? "" + to_replace : to_replace, _metaReplacements[to_replace]);

                model.MetaDescription = string.IsNullOrEmpty(model.MetaDescription) ? "" : model.MetaDescription
                    .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace]) ? " " + to_replace : to_replace, _metaReplacements[to_replace])
                    .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace]) ? "" + to_replace : to_replace, _metaReplacements[to_replace]);

                model.MetaKeyWord = string.IsNullOrEmpty(model.MetaKeyWord) ? "" : model.MetaKeyWord
                    .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace]) ? " " + to_replace : to_replace, _metaReplacements[to_replace])
                    .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace]) ? "" + to_replace : to_replace, _metaReplacements[to_replace]);

                model.MetaTitle = string.IsNullOrEmpty(model.MetaTitle) ? "" : model.MetaTitle
                    .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace]) ? " " + to_replace : to_replace, _metaReplacements[to_replace])
                    .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace]) ? "" + to_replace : to_replace, _metaReplacements[to_replace]);

                model.FooterContent1 = string.IsNullOrEmpty(model.FooterContent1) ? "" : model.FooterContent1
                   .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace]) ? "" + to_replace : to_replace, _metaReplacements[to_replace]);

                model.FooterContent2 = string.IsNullOrEmpty(model.FooterContent2) ? "" : model.FooterContent2
                   .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace]) ? "" + to_replace : to_replace, _metaReplacements[to_replace]);

                model.FooterContent3 = string.IsNullOrEmpty(model.FooterContent3) ? "" : model.FooterContent3
                   .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace]) ? "" + to_replace : to_replace, _metaReplacements[to_replace]);

                model.FooterTitle1 = string.IsNullOrEmpty(model.FooterTitle1) ? "" : model.FooterTitle1
                   .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace]) ? "" + to_replace : to_replace, _metaReplacements[to_replace]);

                model.FooterTitle2 = string.IsNullOrEmpty(model.FooterTitle2) ? "" : model.FooterTitle2
                   .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace]) ? "" + to_replace : to_replace, _metaReplacements[to_replace]);

                model.FooterTitle3 = string.IsNullOrEmpty(model.FooterTitle3) ? "" : model.FooterTitle3
                   .Replace(string.IsNullOrEmpty(_metaReplacements[to_replace]) ? "" + to_replace : to_replace, _metaReplacements[to_replace]);
            }
            return model;
        }
        #endregion

        #region Nested classes

        public partial class FilterElement
        {
            public int BlockId { get; set; }
            public int Id { get; set; }
        }

        #endregion
    }

    public class SpeedFilterSelectedAttr {
        public string CompositeUrl { get; set; }
        public Dictionary<string, string> dictSpecAttr { get; set; }

    }
}
