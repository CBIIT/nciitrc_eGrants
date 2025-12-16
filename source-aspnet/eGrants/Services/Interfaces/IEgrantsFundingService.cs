using eGrants.Models;
using eGrants.ViewModels;

using Microsoft.AspNetCore.Http;

namespace eGrants.Services.Interfaces
{
    /// <summary>
    /// Interface for eGrants Funding service operations
    /// </summary>
    public interface IEgrantsFundingService
    {
        /// <summary>
        /// Loads funding categories by fiscal year
        /// </summary>
        /// <param name="fiscalYear">The fiscal year to filter categories</param>
        /// <returns>List of funding categories</returns>
        Task<List<FundingCategories>> LoadFundingCategoriesAsync(int fiscalYear);

        /// <summary>
        /// Loads funding documents based on action and fiscal year
        /// </summary>
        /// <param name="act">The action type (ViewAll, view_search, view_arra, view_edit)</param>
        /// <param name="serialNum">The serial number to filter</param>
        /// <param name="fiscalYear">The fiscal year</param>
        /// <param name="ic">Institute/Center code</param>
        /// <param name="userId">User ID</param>
        /// <returns>List of funding documents</returns>
        Task<List<FundingDocuments>> LoadFundingDocsAsync(string act, int serialNum, int fiscalYear, string ic, string userId);

        /// <summary>
        /// Creates a new funding document and returns the document ID
        /// </summary>
        /// <param name="applId">Application ID</param>
        /// <param name="categoryId">Category ID</param>
        /// <param name="docDate">Document date</param>
        /// <param name="subCategory">Sub-category name</param>
        /// <param name="fileType">File extension</param>
        /// <param name="ic">Institute/Center code</param>
        /// <param name="userId">User ID</param>
        /// <returns>The created document ID</returns>
        Task<int> GetFundingDocIDAsync(int applId, int categoryId, DateTime docDate, string subCategory, string fileType, string ic, string userId);

        /// <summary>
        /// Loads funding category list without fiscal year filter
        /// </summary>
        /// <returns>List of funding categories</returns>
        Task<List<FundingCategories>> LoadFundingCategoryListAsync();

        /// <summary>
        /// Gets the maximum category ID for the fiscal year
        /// </summary>
        /// <param name="fiscalYear">The fiscal year</param>
        /// <returns>Maximum category ID</returns>
        Task<int> GetMaxCategoryIdAsync(int fiscalYear);

        /// <summary>
        /// Loads all applications by document ID
        /// </summary>
        /// <param name="docId">Document ID</param>
        /// <returns>List of applications</returns>
        Task<List<Appls>> LoadDocApplsAsync(int docId);

        /// <summary>
        /// Loads full grant numbers for applications
        /// </summary>
        /// <param name="serialNum">Serial number</param>
        /// <param name="adminCode">Admin code</param>
        /// <param name="docId">Document ID</param>
        /// <returns>List of applications</returns>
        Task<List<Appls>> LoadFullGrantNumbersAsync(int serialNum, string adminCode, int docId);

        /// <summary>
        /// Edits funding document (delete or restore)
        /// </summary>
        /// <param name="act">Action (delete/restore)</param>
        /// <param name="applId">Application ID</param>
        /// <param name="docId">Document ID</param>
        /// <param name="ic">Institute/Center code</param>
        /// <param name="userId">User ID</param>
        Task EditFundingDocAsync(string act, int applId, int docId, string ic, string userId);

        /// <summary>
        /// Edits funding application association (add/remove)
        /// </summary>
        /// <param name="act">Action (add/remove)</param>
        /// <param name="applId">Application ID</param>
        /// <param name="docId">Document ID</param>
        /// <param name="ic">Institute/Center code</param>
        /// <param name="userId">User ID</param>
        Task EditFundingApplAsync(string act, int applId, int docId, string ic, string userId);

        /// <summary>
        /// Creates funding document by drag and drop
        /// </summary>
        /// <param name="file">The uploaded file</param>
        /// <param name="applId">Application ID</param>
        /// <param name="categoryId">Category ID</param>
        /// <param name="documentDate">Document date</param>
        /// <param name="subCategory">Sub-category</param>
        /// <param name="sessionInfo">Session information</param>
        /// <returns>Result with URL and message</returns>
        Task<FundingDocumentResult> CreateFundingDocByDdropAsync(IFormFile file, int applId, int categoryId, DateTime documentDate, string subCategory, SessionInfo sessionInfo);

        /// <summary>
        /// Creates funding document by file upload
        /// </summary>
        /// <param name="file">The uploaded file</param>
        /// <param name="applId">Application ID</param>
        /// <param name="categoryId">Category ID</param>
        /// <param name="documentDate">Document date</param>
        /// <param name="subCategory">Sub-category</param>
        /// <param name="sessionInfo">Session information</param>
        /// <returns>Result with URL and message</returns>
        Task<FundingDocumentResult> CreateFundingDocByFileAsync(IFormFile file, int applId, int categoryId, DateTime documentDate, string subCategory, SessionInfo sessionInfo);

        /// <summary>
        /// Creates PDF funding document by converting multiple files
        /// </summary>
        /// <param name="files">The uploaded files</param>
        /// <param name="applId">Application ID</param>
        /// <param name="categoryId">Category ID</param>
        /// <param name="documentDate">Document date</param>
        /// <param name="subCategory">Sub-category</param>
        /// <param name="sessionInfo">Session information</param>
        /// <returns>Result with URL and message</returns>
        Task<FundingDocumentResult> CreateFundingPdfByFilesAsync(IEnumerable<IFormFile> files, int applId, int categoryId, DateTime documentDate, string subCategory, SessionInfo sessionInfo);
    }
}