using eGrants.Models;

namespace eGrants.Repositories.Interfaces
{
    public interface IDocumentRepository
    {
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
        /// Asynchronously loads a list of unidentified documents associated with a specific user from the specified image server.
        /// </summary>
        /// <param name="imageServer">The address or identifier of the image server where the documents are stored.</param>
        /// <param name="userId">The unique identifier of the user whose unidentified documents are to be retrieved.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a list of <see cref="DocsUnidentified"/> objects.
        /// </returns>
        Task<List<DocsUnidentified>> LoadDocsUnidentified(string imageServer, string userId);
    }
}
