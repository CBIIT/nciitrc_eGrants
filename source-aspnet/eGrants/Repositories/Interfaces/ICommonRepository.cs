using eGrants.Models;

namespace eGrants.Repositories.Interfaces
{
    public interface ICommonRepository
    {
        public Task<List<AdminCodes>> LoadAdminCodes();
    }
}
