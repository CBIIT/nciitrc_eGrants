using System.Web;

using eGrants.DTOs;
using eGrants.Models;
using eGrants.ViewModels;

namespace eGrants.Services.Interfaces
{
    public interface IDocumentService
    {

        /// <summary>
        /// Retrieves a list of document layers based on application ID and search criteria.
        /// </summary>
        /// <param name="applId">The application ID to filter documents.</param>
        /// <param name="searchType">The type of search to perform (e.g., keyword, category).</param>
        /// <param name="categoryList">A comma-separated list of document categories to include.</param>
        /// <param name="mode">The search mode or context (e.g., view, edit).</param>
        /// <param name="sessionInfo">Session context containing user and environment details.</param>
        /// <returns>A list of <see cref="doclayer"/> objects matching the specified criteria.</returns>
        public List<doclayer> LoadDocs(int applId, string searchType, string categoryList, string mode, ISession sessionInfo);

        /// <summary>
        /// Loads a list of former applications associated with a given grant.
        /// </summary>
        /// <param name="grantId">The unique identifier of the grant.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of former applications.</returns>
        public Task<List<former_appls>> loadFormerAppls(int grantId);

        /// <summary>
        /// Retrieves the default document upload view model for a given document.
        /// </summary>
        /// <param name="docId">The unique identifier of the document.</param>
        /// <returns>A task representing the asynchronous operation, containing the document upload view model.</returns>
        public Task<eGrantsDocUploadViewModel> DocUploadDefaultAsync(int docId);

        /// <summary>
        /// Retrieves the default document update view model for a given document.
        /// </summary>
        /// <param name="docId">The unique identifier of the document.</param>
        /// <param name="previousUrl">The URL of the previous page or resource.</param>
        /// <param name="sessionInfo">The current session information.</param>
        /// <returns>A task representing the asynchronous operation, containing the document update view model.</returns>
        public Task<eGrantsDocUpdateViewModel> DocUpdateDefaultAsync(int docId, string previousUrl, SessionInfo sessionInfo);

        /// <summary>
        /// Creates a new document without associating it with an application ID.
        /// </summary>
        /// <param name="previousUrl">The URL of the previous page or resource.</param>
        /// <param name="sessionInfo">The current session information.</param>
        /// <returns>A task representing the asynchronous operation, containing the document creation view model.</returns>
        public Task<eGrantsDocCreateViewModel> DocCreateWithoutApplIdAsync(string previousUrl, SessionInfo sessionInfo);

        /// <summary>
        /// Uploads a document via drag-and-drop for an existing document ID.
        /// </summary>
        /// <param name="dropedfile">The file dropped by the user.</param>
        /// <param name="docId">The unique identifier of the document.</param>
        /// <param name="sessionInfo">The current session information.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the upload.</returns>
        public Task<DocumentCreateOrUploadResult> DocUploadByDdropAsync(IFormFile dropedfile, int docId, SessionInfo sessionInfo);

        /// <summary>
        /// Uploads a document via file selection for an existing document ID.
        /// </summary>
        /// <param name="file">The file selected by the user.</param>
        /// <param name="docId">The unique identifier of the document.</param>
        /// <param name="sessionInfo">The current session information.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the upload.</returns>
        public Task<DocumentCreateOrUploadResult> DocUploadByFileAsync(IFormFile file, int docId, SessionInfo sessionInfo);

        /// <summary>
        /// Modifies the document index for a given application.
        /// </summary>
        /// <param name="act">The action to perform (e.g., add, update, delete).</param>
        /// <param name="appl_id">The application identifier.</param>
        /// <param name="category_id">The category identifier.</param>
        /// <param name="sub_category">The sub-category name.</param>
        /// <param name="document_date">The date of the document.</param>
        /// <param name="docids">Comma-separated list of document IDs to modify.</param>
        /// <param name="sessionInfo">The current session information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task DocIndexModifyAsync(string act, int appl_id, int category_id, string sub_category, string document_date, string docids, SessionInfo sessionInfo);

        /// <summary>
        /// Loads a list of unidentified documents from the image server for a given user.
        /// </summary>
        /// <param name="imageServer">The image server address.</param>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of unidentified documents.</returns>
        public Task<List<DocsUnidentified>> LoadDocsUnidentified(string imageServer, string userId);

        /// <summary>
        /// Creates a new document via drag-and-drop for a specific application.
        /// </summary>
        /// <param name="dropedfile">The file dropped by the user.</param>
        /// <param name="applId">The application identifier.</param>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="subCategory">The sub-category name.</param>
        /// <param name="docDate">The date of the document.</param>
        /// <param name="adminCode">The administrative code associated with the document.</param>
        /// <param name="serialNum">The serial number of the document.</param>
        /// <param name="sessionInfo">The current session information.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the document creation.</returns>
        public Task<DocumentCreateOrUploadResult> DocCreateByDdropAsync(IFormFile dropedfile, int applId, int categoryId, string subCategory, DateTime docDate, string adminCode, int serialNum, SessionInfo sessionInfo);

        /// <summary>
        /// Creates a new document via file selection for a specific application.
        /// </summary>
        /// <param name="dropedfile">The file selected by the user.</param>
        /// <param name="appl_id">The application identifier.</param>
        /// <param name="category_id">The category identifier.</param>
        /// <param name="sub_category">The sub-category name.</param>
        /// <param name="doc_date">The date of the document.</param>
        /// <param name="admin_code">The administrative code associated with the document.</param>
        /// <param name="serial_num">The serial number of the document.</param>
        /// <param name="sessionInfo">The current session information.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the document creation.</returns>
        public Task<DocumentCreateOrUploadResult> DocCreateByFileAsync(IFormFile dropedfile, int appl_id, int category_id, string sub_category, DateTime doc_date, string admin_code, int serial_num, SessionInfo sessionInfo);

        /// <summary>
        /// Loads a list of categories for a given identifier.
        /// </summary>
        /// <param name="ic">The identifier code used to filter categories.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of categories.</returns>
        public Task<List<CategoriesListDTO>> LoadCategories(string ic);

        /// <summary>
        /// Loads the list of all available sub-categories.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a list of sub-categories.</returns>
        public Task<List<SubCategories>> LoadSubCategoryList();

        /// <summary>
        /// Retrieves the maximum category ID for a given identifier.
        /// </summary>
        /// <param name="ic">The identifier code used to filter categories.</param>
        /// <returns>A task representing the asynchronous operation, containing the maximum category ID.</returns>
        public Task<int> GetMaxCategoryid(string ic);

        /// <summary>
        /// Loads the complete list of available funding categories.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a list of <see cref="FundingCategories"/> objects.
        /// </returns>
        public Task<List<FundingCategories>> LoadFundingCategoryList();


        /// <summary>
        /// Retrieves all uploadable applications associated with a specific application ID.
        /// </summary>
        /// <param name="appl_id">
        /// The unique identifier of the application whose uploadable records are to be retrieved.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a list of <see cref="Appls"/> objects.
        /// </returns>
        public Task<List<Appls>> LoadUploadableApplsByApplid(int appl_id);


        /// <summary>
        /// Gets the unique document ID based on application and document metadata.
        /// </summary>
        /// <param name="applid">The unique identifier of the application.</param>
        /// <param name="categoryid">The identifier of the funding category.</param>
        /// <param name="subcategory">The subcategory name or code associated with the document.</param>
        /// <param name="docdate">The date of the document.</param>
        /// <param name="filetype">The type or extension of the file (e.g., PDF, DOCX).</param>
        /// <param name="ic">An internal code or identifier related to the document.</param>
        /// <param name="userid">The identifier of the user requesting or associated with the document.</param>
        /// <returns>
        /// An integer representing the unique document ID.
        /// </returns>
        public int GetDocID(int applid,
            int categoryid,
            string subcategory,
            DateTime docdate,
            string filetype,
            string ic,
            string userid);

        /// <summary>
        /// Modifies a document record based on the specified action and metadata.
        /// </summary>
        /// <param name="act">The action to perform on the document (e.g., add, update, delete).</param>
        /// <param name="applId">The application identifier associated with the document.</param>
        /// <param name="categoryId">The category identifier to which the document belongs.</param>
        /// <param name="subCategory">The subcategory name or code for further classification.</param>
        /// <param name="docDate">The date of the document, typically in string format (e.g., yyyy-MM-dd).</param>
        /// <param name="docidStr">The unique document identifier string.</param>
        /// <param name="fileType">The type of file (e.g., PDF, DOCX, JPG).</param>
        /// <param name="ic">An additional code or identifier (context-specific, e.g., internal code).</param>
        /// <param name="userId">The identifier of the user performing the modification.</param>
        public void DocModify(string act, int applId, int categoryId, string subCategory, string docDate, string docidStr, string fileType, string ic, string userId);

        /// <summary>
        /// Process document download request and create zip file
        /// </summary>
        /// <param name="request">The download request</param>
        /// <returns>Download model with results</returns>
        Task<DownloadModel> ProcessDocumentDownloadAsync(DownloadRequest request);

        /// <summary>
        ///     The load doc attachments.
        /// </summary>
        /// <param name="document_id">The document_id.</param>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        /// <summary>
        /// Load document attachments asynchronously
        /// </summary>
        /// <param name="documentId">The document_id</param>
        /// <returns>List of document attachments</returns>
        public Task<List<DocAttachment>> LoadDocAttachmentsAsync(int document_id);
    }
}
