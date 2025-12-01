using eGrants.Models;

namespace eGrants.Services.Interfaces
{
    public interface ICommonService
    {
        /// <summary>
        /// Retrieves a list of administrative codes used for categorizing or filtering grant-related data.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="AdminCodes"/> objects.</returns>
        public Task<List<AdminCodes>> LoadAdminCodes();

        public List<CharacterIndex> LoadCharacterIndex();
    }
}
