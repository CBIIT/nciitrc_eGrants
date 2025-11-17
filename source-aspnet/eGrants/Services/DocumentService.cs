using System.Reflection.Metadata;

using eGrants.Models;
using eGrants.Repositories.Interfaces;
using eGrants.Services.Interfaces;
using eGrants.ViewModels;

namespace eGrants.Services
{
    public class DocumentService : IDocumentService
    {
        // Dependency injection of a product repository to access data
        private readonly IDocumentRepository _documentRepository;
        private readonly ISessionInfoService _sessionInfoService;
        private readonly ICommonRepository _commonRepository;
        private readonly IeGrantsService _eGrantsService;

        // Constructor that initializes the repository via dependency injection
        public DocumentService(IDocumentRepository DocumentRepository, ISessionInfoService sessionInfoService, ICommonRepository commonRepository,
            IeGrantsService eGrantsService)
        {
            _documentRepository = DocumentRepository;
            _sessionInfoService = sessionInfoService;
            _commonRepository = commonRepository;
            _eGrantsService = eGrantsService;
        }
        public List<doclayer> LoadDocs(int applId, string searchType, string categoryList, string mode, ISession sessionInfo)
        {
            var session = _sessionInfoService.GetSessionInfo(sessionInfo);

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
        public async Task<List<former_appls>> loadFormerAppls(int grantId)
        {
            return await _documentRepository.loadFormerAppls(grantId);
        }

        public async Task<eGrantsDocUploadViewModel> DocUploadDefaultAsync(int docId)
        {
            var DocInfor = await _documentRepository.GetDocInfo(docId);
            eGrantsDocUploadViewModel eDocViewModel = new eGrantsDocUploadViewModel();
            foreach (var doc in DocInfor)
            {
                eDocViewModel.DocId = doc.document_id;
                eDocViewModel.ApplId = doc.appl_id;
                eDocViewModel.DocName = doc.document_name;
                eDocViewModel.DocDate = doc.document_date;
                eDocViewModel.FullGrantNum = doc.full_grant_num;
            }

            return eDocViewModel;

        }

        public async Task<eGrantsDocUpdateViewModel> DocUpdateDefaultAsync(int docId, string previousUrl, 
            SessionInfo sessionInfo)
        {
            var DocInfor = await _documentRepository.GetDocInfo(docId);
            eGrantsDocUpdateViewModel eDocViewModel = new eGrantsDocUpdateViewModel();

            foreach (var doc in DocInfor)
            {
                eDocViewModel.Act = "Update";
                eDocViewModel.AdminCode = doc.admin_phs_org_code;
                eDocViewModel.SerialNum = doc.serial_num;
                eDocViewModel.ApplId = doc.appl_id;
                eDocViewModel.DocId = doc.document_id;
                eDocViewModel.CategoryId = doc.category_id;
                eDocViewModel.SubCategory = doc.sub_category_name;
                eDocViewModel.DocDate = doc.document_date;
                eDocViewModel.PreviousUrl = previousUrl;
                eDocViewModel.Status = "default";
            }

            int? applId = eDocViewModel.ApplId;
            eDocViewModel.AdminCodeList = await _commonRepository.LoadAdminCodes();
            eDocViewModel.CategoryList = await _documentRepository.LoadCategories(sessionInfo.Ic);
            eDocViewModel.MaxCategoryId = await _documentRepository.GetMaxCategoryId(sessionInfo.Ic);
            eDocViewModel.SubCategoryList = await _documentRepository.LoadSubCategoryList();

            eDocViewModel.GrantYearList = await _eGrantsService.LoadApplsByApplid(applId);

            return eDocViewModel;

        }

    }
}
