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

        public Task<List<former_appls>> loadFormerAppls(int grantId);
        public Task<eGrantsDocUploadViewModel> DocUploadDefaultAsync(int docId);
        public Task<eGrantsDocUpdateViewModel> DocUpdateDefaultAsync(int docId, string previousUrl, SessionInfo sessionInfo);
        public Task<eGrantsDocCreateViewModel> DocCreateWithoutApplIdAsync(string previousUrl, SessionInfo sessionInfo);

        public Task<List<DocsUnidentified>> LoadDocsUnidentified(string imageServer, string userId);

    }
}
