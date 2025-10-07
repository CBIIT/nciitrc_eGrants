using eGrants.Models;
using eGrants.Repositories.Interfaces;
using eGrants.Services.Interfaces;

namespace eGrants.Services
{
    public class DocumentService : IDocumentService
    {
        // Dependency injection of a product repository to access data
        private readonly IDocumentRepository _documentRepository;
        private readonly ISessionInfoService _sessionInfoService;

        // Constructor that initializes the repository via dependency injection
        public DocumentService(IDocumentRepository DocumentRepository, ISessionInfoService sessionInfoService)
        {
            _documentRepository = DocumentRepository;
            _sessionInfoService = sessionInfoService;
        }
        public List<doclayer> LoadDocs(int applId, string searchType, string categoryList, string mode, ISession sessionInfo)
        {
            //return _documentRepository.LoadDocs(aApplId, aSearchType, aCategoryList, aIc, aUserId);

            var session = _sessionInfoService.GetSessionInfo(sessionInfo);
            //var ic = _sessionInfoService.GetSessionInfo("ic");
            //var userid = _sessionInfoService.GetSessionInfo("userid");

            //// Extract session values safely
            //if (!httpContext.Session.TryGetValue("ic", out var icBytes))
            //    sessionInfo.Ic = "";
            //if (!httpContext.Session.TryGetValue("userid", out var userIdBytes))
            //    sessionInfo.UserId = "";

            //if (session.TryGetValue("ic", out var icBytes))
            //    sessionInfo.Ic = "";
            //if (!httpContext.Session.TryGetValue("userid", out var userIdBytes))
            //    sessionInfo.UserId = "";


            // Attempt document loading with retry logic
            const int maxRetries = 5;
            int attempt = 0;
            Exception lastException = null;

            while (attempt < maxRetries)
            {
                try
                {
                    return _documentRepository.LoadDocs(
                        applId,
                        searchType,
                        categoryList,
                        Convert.ToString(session.Ic),
                        Convert.ToString(session.UserId));
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    attempt++;
                }
            }

            // If all retries failed, throw the last exception
            throw lastException ?? new Exception("Unknown error occurred while loading documents.");
        }
    }
}
