using System;
using System.Collections.Generic;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public partial class CategoryModel : BaseNopEntityModel
    {
        public CategoryModel()
        {
            PictureModel = new PictureModel();
            FeaturedProducts = new List<ProductOverviewModel>();
            Products = new List<ProductOverviewModel>();
            PagingFilteringContext = new CatalogPagingFilteringModel();
            SubCategories = new List<SubCategoryModel>();
            CategoryBreadcrumb = new List<CategoryModel>();
            CategoryBannerModels = new List<CategoryBannerModel>();
            FooterContent = new List<Tuple<string, string>>();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }
        
        public PictureModel PictureModel { get; set; }

        public CatalogPagingFilteringModel PagingFilteringContext { get; set; }

        public bool DisplayCategoryBreadcrumb { get; set; }
        public IList<CategoryModel> CategoryBreadcrumb { get; set; }
        
        public IList<SubCategoryModel> SubCategories { get; set; }

        public IList<ProductOverviewModel> FeaturedProducts { get; set; }
        public IList<ProductOverviewModel> Products { get; set; }
        
        public int pictureid { get; set; }
        public int bannerid { get; set; }

        public string HeaderCopy { get; set; }
        public string HeaderTitle { get; set; }
        public string H1Tag { get; set; }
        public string H2Tag { get; set; }
        public string CanonicalUrl { get; set; }
        public string CustomKeyword{ get; set; }
        #region Nested Classes

        public partial class SubCategoryModel : BaseNopEntityModel
        {
            public SubCategoryModel()
            {
                PictureModel = new PictureModel();
                SubCategories = new List<SubCategoryModel>();
            }

            public string Name { get; set; }

            public string SeName { get; set; }

            public string Description { get; set; }

            public PictureModel PictureModel { get; set; }
            public IList<SubCategoryModel> SubCategories { get; set; }
        }

        #endregion

        public IList<CategoryBannerModel> CategoryBannerModels { get; set; }
        public string FooterTitle1 { get; set; }
        public string FooterContent1 { get; set; }
        public string FooterTitle2 { get; set; }
        public string FooterContent2 { get; set; }
        public string FooterTitle3 { get; set; }
        public string FooterContent3 { get; set; }
        public IList<Tuple<string, string>> FooterContent { get; set; }
    }
}