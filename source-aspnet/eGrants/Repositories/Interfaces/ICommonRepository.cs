using eGrants.Models;

namespace eGrants.Repositories.Interfaces
{
    public interface ICommonRepository
    {
        Task<List<AdminCodes>> LoadAdminCodes();
    }
}
