using eGrants.DTOs;
using eGrants.Models;

namespace eGrants.ViewModels
{
    public class eGrantsDocUpdateViewModel
    {
        public string? Act { get; set; }
        public string? AdminCode { get; set; }
        public int? SerialNum { get; set; }
        public short? CategoryId { get; set; }
        public int? DocId { get; set; }
        public int? ApplId { get; set; }
        public DateTime? DocDate { get; set; }
        public string? SubCategory { get; set; }
        public string? PreviousUrl { get; set; }
        public string? Status { get; set; }
        public List<AdminCodes>? AdminCodeList { get; set; }
        public List<CategoriesListDTO>? CategoryList { get; set; }
        public int MaxCategoryId { get; set; }
        public List<SubCategories>? SubCategoryList { get; set; }
        public List<VwApplDTO>? GrantYearList { get; set; }
    }
}
