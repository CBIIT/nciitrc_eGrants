using eGrants.DTOs;
using eGrants.Models;

namespace eGrants.Repositories.Interfaces
{
    public interface IDocumentRepository
    {
        Task<List<DocumentInformation>> GetDocInfo(int docId);

        // TODO:  Consider making this method asynchronous if the underlying data access supports it.

        /// <summary>
        /// Loads a list of document layers associated with a specific application and search criteria.
        /// </summary>
        /// <param name="aApplId">The unique identifier of the application for which documents are being retrieved.</param>
        /// <param name="aSearchType">The type of search to perform (e.g., keyword, category-based).</param>
        /// <param name="aCategoryList">A comma-separated list of document categories to filter the results.</param>
        /// <param name="aIc">The institute or center code associated with the application.</param>
        /// <param name="aUserId">The identifier of the user requesting the documents, used for access control or auditing.</param>
        /// <returns>
        /// A list of <see cref="doclayer"/> objects that match the specified application and search criteria.
        /// </returns>
        List<doclayer> LoadDocs(int aApplId, string aSearchType, string aCategoryList, string aIc, string aUserId);

        /// <summary>
        /// Asynchronously retrieves a list of former applications related to a specific grant.
        /// </summary>
        /// <param name="grantId">The unique identifier of the grant for which former applications are being retrieved.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains a list of <see cref="former_appls"/> objects
        /// associated with the specified grant.
        /// </returns>
        Task<List<former_appls>> loadFormerAppls(int grantId);
        /// <summary>
        /// Asynchronously retrieves a list that could be uploaded by ic and it is for create new only
        /// </summary>
        /// <param name="ic">The unique identifier of the ic for which categories are being retrieved.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains a list of <see cref="Categories"/> objects
        /// associated with the specified grant.
        /// </returns>
        Task<List<CategoriesListDTO>> LoadCategories(string ic);
        /// <summary>
        /// Asynchronously retrieves the maximum category ID for a given IC.
        /// </summary>
        /// <param name="ic">The unique identifier of the ic for which categories are being retrieved.</param>
        ///  <returns>
        /// A task representing the asynchronous operation. The task result contains an int with the max cateogory id
        /// </returns>
        Task<int> GetMaxCategoryId(string ic);
        /// <summary>
        /// Asynchronously retrieves a list of all subcategories from the category subcategory lookup table.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains a list of <see cref="SubCategories"/> objects
        /// representing all available subcategories in the system.
        /// </returns>
        Task<List<SubCategories>> LoadSubCategoryList();

        /// <summary>
        /// Modifies a document's metadata by performing a specified action (store, delete, or update) on the document index.
        /// </summary>
        /// <param name="act">The action to perform on the document (e.g., "store", "delete", "update").</param>
        /// <param name="applId">The unique identifier of the application associated with the document.</param>
        /// <param name="categoryId">The category identifier that classifies the document type.</param>
        /// <param name="subCategory">The subcategory name providing additional classification for the document.</param>
        /// <param name="docDate">The document date in string format.</param>
        /// <param name="docidStr">The document ID as a string, which may represent a single document or multiple documents.</param>
        /// <param name="fileType">The file type or extension of the document (e.g., "pdf", "docx").</param>
        /// <param name="ic">The institute or center code associated with the document.</param>
        /// <param name="userId">The identifier of the user performing the modification, used for auditing purposes.</param>
        void DocModify(string act, int applId, int categoryId, string subCategory, string docDate, string docidStr, string fileType, string ic, string userId);
        /// Asynchronously loads a list of unidentified documents associated with a specific user from the specified image server.
        /// </summary>
        /// <param name="imageServer">The address or identifier of the image server where the documents are stored.</param>
        /// <param name="userId">The unique identifier of the user whose unidentified documents are to be retrieved.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a list of <see cref="DocsUnidentified"/> objects.
        /// </returns>
        Task<List<DocsUnidentified>> LoadDocsUnidentified(string imageServer, string userId);

        /// <summary>
        /// Retrieves the document ID based on the specified criteria.
        /// </summary>
        /// <param name="applid">The application ID associated with the document.</param>
        /// <param name="categoryid">The category ID of the document.</param>
        /// <param name="subcategory">The subcategory of the document.</param>
        /// <param name="docdate">The date of the document.</param>
        /// <param name="filetype">The file type of the document.</param>
        /// <param name="ic">The institute or center code.</param>
        /// <param name="userid">The ID of the user requesting the document ID.</param>
        /// <returns>
        /// A string representing the document ID.
        /// </returns>
        int GetDocID(int applid, int categoryid, string subcategory, DateTime docdate, string filetype, string ic, string userid);

        Task report_doc_error(string errormsg, int docId, string ic, string userId);
    }
}
