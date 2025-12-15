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
        Task<InstitutionalOrg> FindOrg(int orgId, string orgName = "");

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
        Task<List<InstitutionalDocFiles>> LoadOrgDocList(int org_id);

        /// <summary>
        /// Loads a list of institutional organizations associated with the given index.
        /// </summary>
        /// <param name="index_id">The identifier of the index to filter organizations.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="InstitutionalOrg"/> objects.</returns>
        Task<List<InstitutionalOrg>> LoadOrgList(int index_id);

        /// <summary>
        /// Retrieves a list of organization categories.
        /// </summary>
        /// <param name="activeOnly">If true, only active categories are returned; otherwise all categories are included.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="InstitutionalOrgCategory"/> objects.</returns>
        Task<List<InstitutionalOrgCategory>> LoadOrgCategory(bool activeOnly);

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
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated document ID as a string.</returns>
        Task<string> UpdateDocument(int docId, int categoryId, string startDate, string endDate, string ic, string userId, string comments);

        /// <summary>
        /// Disables a document by marking it inactive in the system.
        /// </summary>
        /// <param name="docId">The unique identifier of the document to disable.</param>
        /// <param name="userId">The identifier of the user performing the disable action.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DisableDoc(int docId, string userId);

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
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly created document ID as a string.</returns>
        Task<string> GetDocID(int orgId, int categoryId, string fileType, string startDate, string endDate, string ic, string userId, string comments);

        /// <summary>
        /// Searches for institutional organizations that match the given search string.
        /// </summary>
        /// <param name="searchStr">The search string used to filter organizations by name or attributes.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="InstitutionalOrg"/> objects matching the search criteria.</returns>
        Task<List<InstitutionalOrg>> SearchOrgList(string searchStr);

    }
}
