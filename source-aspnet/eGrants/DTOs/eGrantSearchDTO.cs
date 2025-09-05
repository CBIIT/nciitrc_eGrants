using eGrants.Models;
using eGrants.ViewModels;

namespace eGrants.DTOs
{
    public class eGrantSearchDTO
    {
        public List<eGrantsSearchByStrViewModel> eGrantsSearchResults { get; set; }

        public string Message { get; set; }
        public string grantlayer { get; set; }
        public string Str { get; set; }
        public string Mode { get; set; }
        public int CurrentTab { get; set; }
        public int CurrentPage { get; set; }
        public string SearchStyle { get; set; }

        public List<ApplLayerObject> appllayer { get; set; }
    }
}
