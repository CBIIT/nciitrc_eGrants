using eGrants.DTOs;
using eGrants.Models;

namespace eGrants.Repositories.Interfaces
{
    public interface IInstitutionalFilesRepository
    {
        /// <summary>
        /// Finds and retrieves organizational details based on the provided organization ID and optional name.
        /// </summary>
        /// <param name="orgId">The unique identifier of the organization.</param>
        /// <param name="orgName">An optional name of the organization to refine the search.</param>
        /// <returns>
        /// A task that returns an <see cref="InstFileFindOrgDTO"/> object containing the organization's details.
        /// </returns>
        Task<InstFileFindOrgDTO> FindOrg(int orgId, string orgName = "");

        /// <summary>
        /// Loads a list of character indices used for indexing institutional organization names.
        /// </summary>
        /// <returns>
        /// A task that returns a list of <see cref="InsitutionalOrgNameIndex"/> objects representing name index characters.
        /// </returns>
        Task<List<InsitutionalOrgNameIndex>> LoadOrgNameCharacterIndices();

        /// <summary>
        /// Loads the list of documents associated with a specific organization.
        /// </summary>
        /// <param name="orgId">The unique identifier of the organization.</param>
        /// <returns>
        /// A task that returns a list of <see cref="InstFileLoadOrgDocListDTO"/> objects representing the organization's documents.
        /// </returns>
        Task<List<InstFileLoadOrgDocListDTO>> LoadOrgDocList(int orgId);

    }
}
