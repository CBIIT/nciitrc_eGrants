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

        /// <summary>
        /// Loads all user profiles from the system.
        /// </summary>
        /// <returns>
        /// The <see cref="List{Profiles}"/> containing all available user profiles.
        /// </returns>
        public List<Profiles> LoadProfiles();

        // load postions
        /// <summary>
        ///     The load positions.
        /// </summary>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public List<Position> LoadPositions();

        /// <summary>
        ///     The load coordinators.
        /// </summary>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public List<EgrantsUsers> LoadCoordinators();
    }
}
