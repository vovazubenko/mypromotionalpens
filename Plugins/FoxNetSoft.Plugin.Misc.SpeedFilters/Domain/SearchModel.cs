
namespace FoxNetSoft.Plugin.Misc.SpeedFilters.Domain
{
    public class SearchModel
    {
        /// <summary>
        /// Enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Query string
        /// </summary>
        public string QueryStringForSeacrh { get; set; }

        /// <summary>
        /// Advanced Search
        /// </summary>
        public bool AdvancedSearch { get; set; }

        /// <summary>
        /// CategoryId
        /// </summary>
        public int CategoryId { get; set; }

        //Search.IncludeSubCategories
        public bool IncludeSubCategories { get; set; }

        /// <summary>
        /// ManufacturerId
        /// </summary>
        public int ManufacturerId { get; set; }

        /// <summary>
        /// A value indicating whether to search in descriptions
        /// </summary>
        public bool SearchInDescriptions { get; set; }

        /// <summary>
        /// Price From
        /// </summary>
        public decimal? PriceMin { get; set; }

        /// <summary>
        /// Price To
        /// </summary>
        public decimal? PriceMax { get; set; }

        /// <summary>
        /// RawUrl, ex. ?q=with&adv=true&adv=false&cid=0&isc=false&mid=0&pf=&pt=&sid=true&sid=false
        /// </summary>
        public string RawUrl { get; set; }
    }
}
