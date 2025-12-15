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

        /// <summary>
        /// Retrieves a list of organization categories.
        /// </summary>
        /// <param name="activeOnly">
        /// If true, only active categories are returned; otherwise all categories are included.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains a list of 
        /// <see cref="InstitutionalOrgCategory"/> objects.
        /// </returns>
        public Task<List<InstitutionalOrgCategory>> LoadOrgCategory(bool activeOnly);

        /// <summary>
        /// Loads a list of institutional organizations associated with the given index.
        /// </summary>
        /// <param name="indexId">The identifier of the index to filter organizations.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains a list of 
        /// <see cref="InstitutionalOrg"/> objects.
        /// </returns>
        public Task<List<InstitutionalOrg>> LoadOrgList(int indexId);

        /// <summary>
        /// Updates an existing institutional document with the provided details.
        /// </summary>
        /// <param name="docId">The unique identifier of the document to update.</param>
        /// <param name="categoryId">The category identifier associated with the document.</param>
        /// <param name="startDate">The start date for the document validity period.</param>
        /// <param name="endDate">The end date for the document validity period.</param>
        /// <param name="ic">The institutional code related to the document.</param>
        /// <param name="userId">The identifier of the user performing the update.</param>
        /// <param name="comments">Optional comments about the document update.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the updated 
        /// document ID as a string.
        /// </returns>
        public Task<string> UpdateDocument(int docId, int categoryId, string startDate, string endDate, string ic, string userId, string comments);

        /// <summary>
        /// Disables a document by marking it inactive in the system.
        /// </summary>
        /// <param name="docId">The unique identifier of the document to disable.</param>
        /// <param name="userId">The identifier of the user performing the disable action.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task DisableDoc(int docId, string userId);

        /// <summary>
        /// Creates a new institutional document record and returns its generated ID.
        /// </summary>
        /// <param name="orgId">The organization identifier associated with the document.</param>
        /// <param name="categoryId">The category identifier for the document.</param>
        /// <param name="fileType">The file type or extension of the document.</param>
        /// <param name="startDate">The start date for the document validity period.</param>
        /// <param name="endDate">The end date for the document validity period.</param>
        /// <param name="ic">The institutional code related to the document.</param>
        /// <param name="userId">The identifier of the user creating the document.</param>
        /// <param name="comments">Optional comments about the document creation.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the newly created 
        /// document ID as a string.
        /// </returns>
        public Task<string> GetDocID(int orgId, int categoryId, string fileType, string startDate, string endDate, string ic, string userId, string comments);

        /// <summary>
        /// Searches for institutional organizations that match the given search string.
        /// </summary>
        /// <param name="search_str">The search string used to filter organizations by name or attributes.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains a list of 
        /// <see cref="InstitutionalOrg"/> objects matching the search criteria.
        /// </returns>
        public Task<List<InstitutionalOrg>> SearchOrgList(string search_str);


    }
}
