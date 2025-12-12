using eGrants.DTOs;
using eGrants.Models;

namespace eGrants.Services.Interfaces
{
    public interface IInstitutionalFilesService
    {
        /// <summary>
        /// Finds a specific institutional organization by its ID and optionally by name.
        /// </summary>
        /// <param name="orgId">The unique identifier of the organization.</param>
        /// <param name="orgName">Optional name of the organization to refine the search.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the matching <see cref="InstitutionalOrg"/> object, or null if not found.</returns>
        public Task<InstitutionalOrg> FindOrg(int orgId, string orgName = "");


        /// <summary>
        /// Loads all character index records used for organizing institutional organization names.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="InsitutionalOrgNameIndex"/> objects.</returns>
        public Task<List<InsitutionalOrgNameIndex>> LoadOrgNameCharacterIndices();

        /// <summary>
        /// Loads a list of institutional organizations associated with a specific index ID.
        /// </summary>
        /// <param name="indexId">The index ID used to filter the organizations.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="InstitutionalOrg"/> objects.</returns>
        public Task<List<InstitutionalDocFiles>> LoadOrgDocList(int orgId);

        public Task<List<InstitutionalOrgCategory>> LoadOrgCategory(bool activeOnly);

        public Task<List<InstitutionalOrg>> LoadOrgList(int indexId);

        public Task<string> UpdateDocument(int docId, int categoryId, string startDate, string endDate, string ic, string userId, string comments);

        public Task DisableDoc(int docId, string userId);

        public Task<string> GetDocID(int orgId, int categoryId, string fileType, string startDate, string endDate, string ic, string userId, string comments);

    }
}
