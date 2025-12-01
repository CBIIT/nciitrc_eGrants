using eGrants.Models;

namespace eGrants.Repositories.Interfaces
{
    public interface ICommonRepository
    {
        /// <summary>
        /// Asynchronously retrieves a list of administrative codes used for classification, routing, or organizational purposes.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains a list of <see cref="AdminCodes"/> objects
        /// available in the system.
        /// </returns>
        public Task<List<AdminCodes>> LoadAdminCodes();

        public List<AdminMenus> LoadAdminMenus(string userid);
    }
}
