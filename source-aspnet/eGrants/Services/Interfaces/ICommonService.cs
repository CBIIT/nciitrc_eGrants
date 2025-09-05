using eGrants.Models;

namespace eGrants.Services.Interfaces
{
    public interface ICommonService
    {
        public Task<List<AdminCodes>> LoadAdminCodes();
    }
}
