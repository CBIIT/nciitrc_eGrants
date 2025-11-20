using System.Reflection.Metadata;
using System.Web;
using System.Xml.Serialization;
using System.IO;
using Microsoft.AspNetCore.Http;

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
                eDocViewModel.DocDate = doc.document_date.HasValue ? doc.document_date.Value.ToString("MM/dd/yyyy") : string.Empty;
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
                eDocViewModel.DocDate = doc.document_date.HasValue ? doc.document_date.Value.ToString("MM/dd/yyyy") : string.Empty;
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

        public async Task<eGrantsDocCreateViewModel> DocCreateWithoutApplIdAsync(string previousUrl,
            SessionInfo sessionInfo)
        {
            eGrantsDocCreateViewModel eDocViewModel = new eGrantsDocCreateViewModel
            {
                Act = "Add",
                AdminCodeList = await _commonRepository.LoadAdminCodes(),
                CategoryList = await _documentRepository.LoadCategories(sessionInfo.Ic),
                MaxCategoryId = await _documentRepository.GetMaxCategoryId(sessionInfo.Ic),
                SubCategoryList = await _documentRepository.LoadSubCategoryList(),
                PreviousUrl = previousUrl
            };

            return eDocViewModel;

        }

        //TO BE IMPLEMENTED LATER in ADD DOCUMENT FUNCTIONALITY TICKET
        //public async Task<DocumentCreateOrUploadResult> DocCreateByDdropAsync(IFormFile dropedfile,
        //    int applId,
        //    int categoryId,
        //    string subCategory,
        //    DateTime docDate,
        //    string adminCode,
        //    int serialNum,
        //    SessionInfo sessionInfo)
        //{
        //    var result = new DocumentCreateOrUploadResult();
        //    var docName = string.Empty;

        //    if (dropedfile != null && dropedfile.Length > 0)
        //    {
        //        try
        //        {
        //            // Get file name and file extension
        //            var fileName = Path.GetFileName(dropedfile.FileName);
        //            var fileExtension = Path.GetExtension(fileName);

        //            // Get document_id and create a new docName
        //            //var documentId = await _documentRepository.GetDocId(
        //            //    applId,
        //            //    categoryId,
        //            //    subCategory,
        //            //    docDate,
        //            //    fileExtension,
        //            //    sessionInfo.Ic,
        //            //    sessionInfo.UserId);

        //            //docName = Convert.ToString(documentId) + fileExtension;

        //            var fileFolder = @"\\" + sessionInfo.WebGrantUrl + "\\egrants\\funded2\\nci\\main\\";
        //            var filePath = Path.Combine(fileFolder, docName);

        //            // Save the file
        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await dropedfile.CopyToAsync(stream);
        //            }

        //            // Create review url
        //            var fileUrl = sessionInfo.ImageServerUrl + sessionInfo.EgrantsDocNewRelativePath + docName;

        //            result.Success = true;
        //            result.Url = fileUrl;
        //            result.Message = "Done! New document has been created";
        //            //result.DocumentId = documentId;
        //        }
        //        catch (Exception ex)
        //        {
        //            result.Success = false;
        //            result.Url = null;
        //            result.Message = "ERROR:" + ex.Message;
        //        }
        //    }
        //    else
        //    {
        //        result.Success = false;
        //        result.Url = null;
        //        result.Message = "You have not specified a file.";
        //    }

        //    return result;
        //}

        public async Task<DocumentCreateOrUploadResult> DocUploadByDdropAsync(IFormFile dropedfile, int docId, SessionInfo sessionInfo)
        {
            var result = new DocumentCreateOrUploadResult();
            var docName = string.Empty;

            if (dropedfile != null && dropedfile.Length > 0)
            {
                try
                {
                    // Get file name and file extension
                    var fileName = Path.GetFileName(dropedfile.FileName);
                    var fileExtension = Path.GetExtension(fileName);

                    // Get document id and create new document name
                    docName = Convert.ToString(docId) + fileExtension;

                    //Update url for document
                    _documentRepository.DocModify(
                       "to_upload",
                       0,
                       0,
                       string.Empty,
                       string.Empty,
                       Convert.ToString(docId),
                       fileExtension,
                       sessionInfo.Ic,
                       sessionInfo.UserId);

                    var fileFolder = @"\\" + sessionInfo.WebGrantUrl + "\\egrants\\funded\\nci\\modify\\";
                    var filePath = Path.Combine(fileFolder, docName);
                    // Save the file using FileStream
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dropedfile.CopyToAsync(stream);
                    }

                    // Create review url
                    var fileUrl = sessionInfo.ImageServerUrl + sessionInfo.EgrantsDocModifyRelativePath + docName;

                    result.Success = true;
                    result.Url = fileUrl;
                    result.Message = "Done! New document has been created";
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Url = null;
                    result.Message = "ERROR:" + ex.Message;
                }
            }
            else
            {
                result.Success = false;
                result.Url = null;
                result.Message = "Error while uploading the files.";
            }

            return result;
        }

    public async Task<DocumentCreateOrUploadResult> DocUploadByFileAsync(IFormFile file, int docId, SessionInfo sessionInfo)
        {
            var result = new DocumentCreateOrUploadResult();
            var docName = string.Empty;

            if (file != null && file.Length > 0)
            {
                try
                {
                    // Get file name and file extension
                    var fileName = Path.GetFileName(file.FileName);
                    var fileExtension = Path.GetExtension(fileName);

                    // Update url for document
                    _documentRepository.DocModify(
                        "to_upload",
                        0,
                        0,
                        string.Empty,
                        string.Empty,
                        Convert.ToString(docId),
                        fileExtension,
                        sessionInfo.Ic,
                        sessionInfo.UserId);

                    // Get document id and create new document name
                    docName = Convert.ToString(docId) + fileExtension;

                    var fileFolder = @"\\" + sessionInfo.WebGrantUrl + "\\egrants\\funded\\nci\\modify\\";
                    var filePath = Path.Combine(fileFolder, docName);

                    // Save the file using FileStream
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Create review url
                    var fileUrl = sessionInfo.ImageServerUrl + sessionInfo.EgrantsDocModifyRelativePath + docName;

                    result.Success = true;
                    result.Url = fileUrl;
                    result.Message = "Done! New document has been created";
                    result.DocumentId = docId;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Url = null;
                    result.Message = "ERROR:" + ex.Message;
                }
            }
            else
            {
                result.Success = false;
                result.Url = null;
                result.Message = "Error while uploading the files.";
            }

            return result;
        }

        public async Task DocIndexModifyAsync(string act, int applId, int categoryId, string subCategory, string documentDate, string docIds, SessionInfo sessionInfo)
        {
            await Task.Run(() =>
            {
                _documentRepository.DocModify(
                    act,
                    applId,
                    categoryId,
                    subCategory,
                    documentDate,
                    docIds,
                    string.Empty,
                    sessionInfo.Ic,
                    sessionInfo.UserId);
            });

        }

        public async Task<List<DocsUnidentified>> LoadDocsUnidentified(string imageServer, string userId)
        {
            return await _documentRepository.LoadDocsUnidentified(imageServer, userId);
        }
    }
}
