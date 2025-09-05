using eGrants.Models;

namespace eGrants.ViewModels
{
    public class GrantViewModel
    {
        public string grant_id { get; set; }
        public List<ApplLayerObject> appllayer { get; set; }
    }
}
