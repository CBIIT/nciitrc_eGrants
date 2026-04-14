using System.Data;
using System.Net.Http;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

using eGrants.DAL;
using eGrants.DTOs;
using eGrants.Models;
using eGrants.Repositories;
using eGrants.Repositories.Interfaces;
using eGrants.Services.Interfaces;
using eGrants.ViewModels;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Serilog;

namespace eGrants.Services
{
    /// <summary>
    /// Document Service - Handles all document-related business logic
    /// 
    /// MIGRATION CHANGES SUMMARY:
    /// -------------------------
    /// This service was created as part of the .NET Framework 4.8 to .NET 8 migration.
    /// Previously, this logic existed inline in controllers or in static helper classes.
    /// 
    /// 1. SERVICE LAYER ARCHITECTURE:
    ///    WHY: Moving business logic from controllers to services follows the Single
    ///    Responsibility Principle. Controllers should only handle HTTP concerns,
    ///    while services contain business logic. This enables:
    ///    - Unit testing without HTTP context
    ///    - Reuse of logic across multiple controllers
    ///    - Cleaner separation of concerns
    /// 
    /// 2. ERA CONNECTION HANDLING (ProcessDocumentDownloadAsync, HandleEraFileAsync):
    ///    WHY: The ERA (eRA Commons) service requires client certificate authentication.
    ///    Several changes were necessary:
    ///    - SemaphoreSlim throttling: ERA service can reject rapid concurrent connections.
    ///      Limiting to 3 concurrent connections prevents server-side throttling/blocking.
    ///    - SocketsHttpHandler: Provides better TLS 1.2/1.3 control than HttpClientHandler.
    ///      .NET 8 deprecates older TLS protocols for security reasons.
    ///    - X509KeyStorageFlags: MachineKeySet + PersistKeySet + Exportable are REQUIRED
    ///      for web applications running under IIS/Kestrel to access private keys.
    ///      Without these flags, certificate authentication fails silently.
    ///    - Retry logic with exponential backoff: Network transients happen; retrying
    ///      with increasing delays (2s, 4s, 8s) handles temporary connection issues.
    ///    - Comprehensive Serilog logging: Debugging SSL/TLS issues requires detailed logs.
    /// 
    /// 3. PDF GENERATION (GenerateCloseoutNotificationPdf):
    /// WHY: Legacy used Rotativa/wkhtmltopdf which requires native binaries.
    ///    .NET 8 on Linux containers or Azure App Services may not have wkhtmltopdf.
    ///    EmailConcatenation.PdfConverter provides pure .NET PDF generation using IronPdf.
    /// 
    /// 4. FILE PATH HANDLING:
    ///    WHY: The service receives SessionInfo with file server paths.
    ///    Paths are constructed dynamically from configuration rather than hardcoded,
    ///    allowing different environments (Dev/Test/Stage/Prod) to use different servers.
    /// 
    /// 5. ASYNC PATTERNS:
    ///    WHY: File I/O, HTTP requests, and database operations are I/O-bound.
    ///    Using async/await frees up threads while waiting, improving scalability.
    ///    A single server can handle more concurrent requests with async I/O.
    /// 
    /// 6. REPOSITORY PATTERN:
    ///    WHY: Database access is abstracted through IDocumentRepository.
    ///    This allows swapping data access implementations and mocking for tests.
    /// </summary>
    public class DocumentService : IDocumentService
    {
        // Dependency injection of a product repository to access data
        private readonly IDocumentRepository _documentRepository;
        private readonly ISessionInfoService _sessionInfoService;
        private readonly ICommonRepository _commonRepository;
        private readonly IeGrantsService _eGrantsService;
        private readonly AppDbContext _context;

        // Static semaphore to limit concurrent ERA connections (prevents server-side throttling)
     // WHY: ERA services may reject or throttle rapid concurrent requests.
        // Limiting to 3 concurrent connections provides a balance between throughput
        // and avoiding server-side rate limiting or connection resets.
        private static readonly SemaphoreSlim _eraConnectionSemaphore = new SemaphoreSlim(3, 3);

        // Static HttpClient for ERA connections - reusing connections reduces SSL handshake overhead
        // WHY: Creating new HttpClient instances for each request causes socket exhaustion.
        // Static/shared handlers enable connection pooling and reduce TLS negotiation overhead.
        private static readonly Lazy<HttpClientHandler> _eraHandler = new Lazy<HttpClientHandler>(() =>
        {
            var handler = new HttpClientHandler
            {
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    if (sslPolicyErrors != System.Net.Security.SslPolicyErrors.None)
                    {
                        Log.Warning("ERA SSL Certificate Issue - Errors: {SslErrors}, Subject: {Subject}",
                            sslPolicyErrors, cert?.Subject);
                    }
                    return true;
                },
                // Enable connection pooling
                MaxConnectionsPerServer = 5
            };
            return handler;
        });

        // Constructor that initializes the repository via dependency injection
        public DocumentService(IDocumentRepository DocumentRepository, ISessionInfoService sessionInfoService, ICommonRepository commonRepository,
            IeGrantsService eGrantsService, AppDbContext context = null)
        {
            _documentRepository = DocumentRepository;
            _sessionInfoService = sessionInfoService;
            _commonRepository = commonRepository;
            _eGrantsService = eGrantsService;
            _context = context ?? throw new ArgumentNullException(nameof(context));
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

        public async Task<DocumentCreateOrUploadResult> DocCreateByDdropAsync(IFormFile dropedfile,
            int applId,
            int categoryId,
            string subCategory,
            DateTime docDate,
            string adminCode,
            int serialNum,
            SessionInfo sessionInfo)
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

                    // Get document_id and create a new docName
                    var documentId = _documentRepository.GetDocID(
                        applId,
                        categoryId,
                        subCategory,
                        docDate,
                        fileExtension,
                        sessionInfo.Ic,
                        sessionInfo.UserId);

                    docName = Convert.ToString(documentId) + fileExtension;

                    var fileFolder = @"\\" + sessionInfo.WebGrantUrl + "\\egrants\\funded2\\nci\\main\\";
                    var filePath = Path.Combine(fileFolder, docName);

                    // Save the file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dropedfile.CopyToAsync(stream);
                    }

                    // Create review url
                    var fileUrl = sessionInfo.ImageServerUrl + sessionInfo.EgrantsDocNewRelativePath + docName;

                    result.Success = true;
                    result.Url = fileUrl;
                    result.Message = "Done! New document has been created";
                    result.DocumentId = documentId;
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
                result.Message = "You have not specified a file.";
            }

            return result;
        }

        public async Task<DocumentCreateOrUploadResult> DocCreateByFileAsync(
            IFormFile file,
            int appl_id,
            int category_id,
            string sub_category,
            DateTime doc_date,
            string admin_code,
            int serial_num,
            SessionInfo sessionInfo)
        {
            var result = new DocumentCreateOrUploadResult();
            var docName = string.Empty;

            if (file != null && file.Length > 0)
                try
                {
                    // get file name and file Extension
                    var fileName = Path.GetFileName(file.FileName);
                    var fileExtension = Path.GetExtension(fileName);

                    // get document_id and creat a new docName
                    var document_id = _documentRepository.GetDocID(appl_id, category_id, sub_category,
                        doc_date, fileExtension,
                        sessionInfo.Ic, sessionInfo.UserId);

                    docName = Convert.ToString(document_id) + fileExtension;

                    // upload to image sever 
                    var fileFolder = @"\\" + sessionInfo.WebGrantUrl + "\\egrants\\funded2\\nci\\main\\";

                    var filePath = Path.Combine(fileFolder, docName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // create review url
                    result.Url = sessionInfo.ImageServerUrl + sessionInfo.EgrantsDocNewRelativePath + Convert.ToString(docName);
                    result.Message = "Done! New document has been created";
                }
                catch (Exception ex)
                {
                    result.Message = "ERROR:" + ex.Message;
                }
            else
                result.Message = "You have not specified a file.";

            return result;
        }

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

        public async Task DocIndexModifyAsync(string act, int applId, int categoryId, string subCategory, string docDate, string docIds, SessionInfo sessionInfo)
        {
            await Task.Run(() =>
            {
                _documentRepository.DocModify(
                    act,
                    applId,
                    categoryId,
                    subCategory,
                    docDate,
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

        public async Task<List<CategoriesListDTO>> LoadCategories(string ic)
        {
     var list = new List<CategoriesListDTO>();

            try
            {
           return await _documentRepository.LoadCategories(ic);
      }
     catch (Exception ex)
  {
       Log.Error(ex, "Error loading categories for IC: {IC}", ic);
        }

    return list;
    }

      public async Task<List<SubCategories>> LoadSubCategoryList()
        {
   var list = new List<SubCategories>();

 try
    {
         return await _documentRepository.LoadSubCategoryList();
            }
    catch (Exception ex)
            {
       Log.Error(ex, "Error loading subcategories");
       }

            return list;
        }

        public async Task<int> GetMaxCategoryid(string ic)
        {
            var maxCategoryid = 0;

       try
    {
          return await _documentRepository.GetMaxCategoryId(ic);
            }
            catch (Exception ex)
    {
           Log.Error(ex, "Error getting max category id for IC: {IC}", ic);
  }

     return maxCategoryid;
        }

public async Task<List<FundingCategories>> LoadFundingCategoryList()
        {
            var conn = new SqlConnection(_context.Database.GetConnectionString());

   var cmd = new SqlCommand("SELECT distinct category_id,category_name,level_id,parent_id FROM funding_categories " +
    "WHERE category_fy is null or category_fy = 2014 Order by level_id, category_name",
           conn);

    cmd.CommandType = CommandType.Text;

          conn.Open();

            var list = new List<FundingCategories>();
    var rdr = cmd.ExecuteReader();

            while (rdr.Read())
   {
      list.Add(new FundingCategories
     {
         category_id = rdr["category_id"]?.ToString(),
            category_name = rdr["category_name"]?.ToString(),
level_id = rdr["level_id"]?.ToString(),
        parent_id = rdr["parent_id"]?.ToString()
     });
   }

            conn.Close();

            return list;
        }

        public async Task<List<Appls>> LoadUploadableApplsByApplid(int appl_id)
        {
     var conn = new SqlConnection(_context.Database.GetConnectionString());

    var cmd = new SqlCommand(
     "select appl_id, support_year, full_grant_num from vw_appls "
              + " where grant_id = (select grant_id from appls where appl_id = @applid) and frc_destroyed=0 and deleted_by_impac='n' order by support_year desc",
     conn);

     cmd.CommandType = CommandType.Text;
     cmd.Parameters.Add("@applid", SqlDbType.Int).Value = appl_id;
        conn.Open();

  var GrantYearList = new List<Appls>();
            var rdr = cmd.ExecuteReader();

    while (rdr.Read())
            {
        GrantYearList.Add(
           new Appls
     {
              appl_id = rdr["appl_id"]?.ToString(),
       support_year = rdr["support_year"]?.ToString(),
full_grant_num = rdr["full_grant_num"]?.ToString()
        });
            }

            rdr.Close();
            conn.Close();

     return GrantYearList;
        }

        public int GetDocID(
            int applid,
         int categoryid,
            string subcategory,
   DateTime docdate,
          string filetype,
      string ic,
        string userid)
        {
            return _documentRepository.GetDocID(
        applid,
       categoryid,
    subcategory,
    docdate,
     filetype,
            ic,
       userid);
      }

        public void DocModify(string act, int applId, int categoryId, string subCategory, string docDate, string docidStr, string fileType, string ic, string userId)
        {
 _documentRepository.DocModify(act, applId, categoryId, subCategory, docDate, docidStr, fileType, ic, userId);
        }

        /// <summary>
   /// Process document download request and create zip file
        /// </summary>
        /// <param name="request">The download request</param>
        /// <returns>Download model with results</returns>
        public async Task<DownloadModel> ProcessDocumentDownloadAsync(DownloadRequest request)
        {
            var downloadModel = new DownloadModel
  {
          ApplId = request.ApplId,
  NumFailed = 0,
       NumSucceeded = 0,
      NumToDownload = request.ListOfUrl?.Count ?? 0,
      DownloadDataList = new List<DownloadData>()
       };

  string downloadDirectory;

          try
  {
  if (request.ListOfUrl == null || !request.ListOfUrl.Any())
            {
    downloadModel.Error = "There are no URLs in the list!";
          return downloadModel;
       }

     downloadDirectory = Path.Combine(Path.GetTempPath(), request.ApplId);
          var directoryInfo = Directory.CreateDirectory(downloadDirectory);

         // Delete all files in directory
       foreach (var file in directoryInfo.GetFiles())
    {
           file.Delete();
    }

       // Delete all folders in directory
       foreach (var dir in directoryInfo.GetDirectories())
            {
     dir.Delete(true);
   }
  }
            catch (ArgumentNullException)
       {
         downloadModel.Error = "There are no URLs in the list!";
  return downloadModel;
            }
            catch (Exception)
       {
        downloadModel.Error = "General Exception. This is likely an error in accessing temp files and temp directories! Notify Development Team of this error.";
              return downloadModel;
         }

  foreach (var dataInput in request.ListOfUrl)
      {
  var downloadData = new DownloadData();
    var diagnostics = new System.Text.StringBuilder();

         try
   {
     var split = dataInput.Split('|', StringSplitOptions.None);

      var url = split[0];
           var category = split[1];
     var subCategory = split[2];
     var documentId = split[3];
    var documentName = split[4];
         var documentDate = split[5];

          downloadData.Url = url;
downloadData.Category = category;
      downloadData.SubCategory = subCategory;
    downloadData.DocumentId = string.IsNullOrEmpty(documentId) ? 0 : Convert.ToInt32(documentId);
        downloadData.DocumentName = documentName;
  downloadData.DocumentDate = DateTime.TryParse(documentDate, out var result) ? result : null;

       var tmpFileName = Path.GetTempFileName();

          // Skip i2e files
         if (url.Contains("https://i2e"))
    {
   throw new Exception("We found an i2e path and these should not be included in downloads");
  }

 // Handle ERA Server files
          if (url.Contains("https://services."))
           {
 var cerUri = request.SessionInfo.CertPath;
      var certPass = request.SessionInfo.CertPass;
           X509Certificate2 certificate = null;

       if (!string.IsNullOrEmpty(cerUri) && System.IO.File.Exists(cerUri))
         {
        // Load certificate with proper key storage flags for ASP.NET Core / .NET 8
              // MachineKeySet: Store keys in the machine key store (required for IIS/web apps)
        // PersistKeySet: Persist the key after the certificate is loaded
   // Exportable: Allow the private key to be exported (needed for SSL client auth)
           certificate = new X509Certificate2(
      cerUri,
   certPass,
          X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

              diagnostics.Append("Handling as era service. ");
          var resultStatus = await HandleEraFileAsync(url, tmpFileName, certificate, downloadDirectory, request.FullGrantNumber,
          category, documentName, documentDate, documentId, downloadData, diagnostics);

     if (!resultStatus)
          {
         downloadModel.NumFailed += 1;
           downloadData.Error = "File not found or certificate issues";
        }
   else
            {
      downloadModel.NumSucceeded += 1;
              }
     }
  else
             {
        Log.Warning("Certificate not found at path: {CertPath}", cerUri);
   downloadModel.NumFailed += 1;
 downloadData.Error = "File not found.";
           }
          }
            else
     {
       diagnostics.Append("Not era file. ");
 var uri = CreateUri(url, request.SessionInfo.ImageServerUrl, diagnostics);
              diagnostics.Append("Completed uri creation. ");

      if (category == "CloseoutNotification" || category == "FFR_REJECTION")
{
     var resultStatus = await HandleCloseoutNotificationAsync(category, request.ApplId, documentName, tmpFileName,
      downloadDirectory, request.FullGrantNumber, documentDate, downloadData, diagnostics, request.SessionInfo);

    if (!resultStatus)
   {
      downloadModel.NumFailed += 1;
       downloadData.Error += "File not found";
           }
     else
{
downloadModel.NumSucceeded += 1;
      }
           }
  else
         {
     await HandleStandardFileAsync(uri, tmpFileName, downloadDirectory, request.FullGrantNumber,
          documentName, documentId, downloadData, diagnostics, request.SessionInfo);
       downloadModel.NumSucceeded += 1;
   }
             }
          }
     catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
  {
         downloadData.Error = "File not found.";
       downloadModel.NumFailed += 1;
}
    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.InternalServerError)
     {
       downloadData.Error = "Internal Server Error! Notify Dev Team!";
       downloadModel.NumFailed += 1;
         }
           catch (ArgumentNullException)
{
         downloadData.Error = "A value is null which should not be.";
     downloadModel.NumFailed += 1;
    }
         catch (Exception err)
     {
    downloadData.Error = "General Exception! Screenshot this message and notify the Development Team: "
     + Environment.NewLine + err.Message + diagnostics.ToString();
  downloadModel.NumFailed += 1;
          Log.Error(Convert.ToString(err.InnerException));
     }

    downloadModel.DownloadDataList.Add(downloadData);
          }

            // Create zip file
            var handle = Guid.NewGuid().ToString();
        downloadModel.Handle = handle;

            var zipFileName = request.FullGrantNumber.Remove(0, 1) + ".zip";
            var zipFileNameWithPath = Path.Combine(Path.GetTempPath(), zipFileName);
        downloadModel.ZipFilename = zipFileName;

            try
{
        if (System.IO.File.Exists(zipFileNameWithPath))
    {
     System.IO.File.Delete(zipFileNameWithPath);
       }

        System.IO.Compression.ZipFile.CreateFromDirectory(downloadDirectory, zipFileNameWithPath);

    using (var ms = new MemoryStream())
   using (var file = new FileStream(zipFileNameWithPath, FileMode.Open, FileAccess.Read))
                {
    var bytes = new byte[file.Length];
await file.ReadAsync(bytes, 0, (int)file.Length);
         await ms.WriteAsync(bytes, 0, (int)file.Length);
           downloadModel.ZipFileBytes = ms.ToArray();
  }
      }
 catch (Exception err)
            {
        downloadModel.Error = "ZIP FILE ERROR! Screenshot this error and send to Dev team! "
        + Environment.NewLine + err.ToString();
 }

       return downloadModel;
        }

        /// <summary>
     /// Handle ERA service file download using HttpClientHandler with rate limiting
   /// </summary>
        private async Task<bool> HandleEraFileAsync(
        string url,
  string tmpFileName,
            X509Certificate2 certificate,
            string downloadDirectory,
            string fullGrantNumber,
         string category,
      string documentName,
  string documentDate,
        string documentId,
 DownloadData downloadData,
      System.Text.StringBuilder diagnostics)
        {
  var uri = new Uri(url);
          var requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
    var startTime = DateTime.UtcNow;

            diagnostics.Append($"[{requestId}] Uri created: {uri.Host}{uri.AbsolutePath}. ");

Log.Information("ERA Download START - RequestId: {RequestId}, URL: {Url}, DocumentId: {DocumentId}, Category: {Category}, DocumentName: {DocumentName}",
      requestId, url, documentId, category, documentName);

         const int maxRetries = 3;
        int attempt = 0;
      Exception lastException = null;

            // Acquire semaphore to limit concurrent connections (prevents server-side throttling)
            diagnostics.Append($"Waiting for connection slot. ");
  await _eraConnectionSemaphore.WaitAsync();
     var semaphoreAcquiredTime = DateTime.UtcNow;
            var waitDuration = semaphoreAcquiredTime - startTime;
    diagnostics.Append($"Got slot after {waitDuration.TotalMilliseconds:F0}ms. ");

    if (waitDuration.TotalMilliseconds > 100)
            {
                Log.Information("ERA Connection Throttled - RequestId: {RequestId}, WaitTime: {WaitTime}ms",
     requestId, waitDuration.TotalMilliseconds);
 }

      try
        {
                while (attempt < maxRetries)
         {
        attempt++;
        var attemptStartTime = DateTime.UtcNow;
                  diagnostics.Append($"[{requestId}] Attempt {attempt} at {attemptStartTime:HH:mm:ss.fff}. ");

 try
 {
           // Create a SocketsHttpHandler for better TLS control in .NET 8
            // This provides more reliable SSL/TLS handling than HttpClientHandler
     var handler = new SocketsHttpHandler
        {
           SslOptions = new System.Net.Security.SslClientAuthenticationOptions
 {
       // Explicitly enable TLS 1.2 (and optionally TLS 1.3)
 EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
     // Add client certificate for mutual TLS authentication
   ClientCertificates = new X509Certificate2Collection { certificate },
       // Accept server certificate (customize as needed for production)
      RemoteCertificateValidationCallback = (message, cert, chain, sslPolicyErrors) =>
         {
              if (sslPolicyErrors != System.Net.Security.SslPolicyErrors.None)
                  {
     Log.Warning("ERA SSL Certificate Warning - RequestId: {RequestId}, Attempt: {Attempt}, Errors: {SslErrors}, Subject: {Subject}, Issuer: {Issuer}",
     requestId, attempt, sslPolicyErrors, cert?.Subject, cert?.Issuer);
              }
   return true;
          }
 },
           // Connection pooling settings
                MaxConnectionsPerServer = 5,
PooledConnectionLifetime = TimeSpan.FromMinutes(2),
    ConnectTimeout = TimeSpan.FromSeconds(30)
          };

          if (certificate != null)
      {
             diagnostics.Append($"Certificate loaded: {certificate.Subject}, HasPrivateKey: {certificate.HasPrivateKey}. ");

           using var client = new HttpClient(handler);
           client.Timeout = TimeSpan.FromMinutes(5);

       // First request - get the download URL
  var response = await client.GetAsync(uri);
     var firstRequestDuration = DateTime.UtcNow - attemptStartTime;

 Log.Information("ERA First Request - RequestId: {RequestId}, Attempt: {Attempt}, StatusCode: {StatusCode}, Duration: {Duration}ms",
            requestId, attempt, response.StatusCode, firstRequestDuration.TotalMilliseconds);

    response.EnsureSuccessStatusCode();

             var downloadUrl = await response.Content.ReadAsStringAsync();
        diagnostics.Append($"Got download URL ({downloadUrl.Length} chars). ");

                 // Small delay between first request and file download to avoid overwhelming the server
       await Task.Delay(100);

   // Create a new handler for the download
             var downloadHandler = new SocketsHttpHandler
       {
          SslOptions = new System.Net.Security.SslClientAuthenticationOptions
        {
               EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
         ClientCertificates = new X509Certificate2Collection { certificate },
         RemoteCertificateValidationCallback = (message, cert, chain, sslPolicyErrors) =>
    {
      if (sslPolicyErrors != System.Net.Security.SslPolicyErrors.None)
      {
              Log.Warning("ERA File Download SSL Warning - RequestId: {RequestId}, Attempt: {Attempt}, Errors: {SslErrors}",
      requestId, attempt, sslPolicyErrors);
            }
   return true;
               }
          }
              };

             using var downloadClient = new HttpClient(downloadHandler);
        downloadClient.DefaultRequestHeaders.Add("User-Agent", "eGrants");
         downloadClient.Timeout = TimeSpan.FromMinutes(10);

           var fileDownloadStart = DateTime.UtcNow;
         var fileResponse = await downloadClient.GetAsync(downloadUrl);
      var fileDownloadDuration = DateTime.UtcNow - fileDownloadStart;

           var contentLength = fileResponse.Content.Headers.ContentLength;
       Log.Information("ERA File Download - RequestId: {RequestId}, Attempt: {Attempt}, StatusCode: {StatusCode}, ContentLength: {ContentLength}, Duration: {Duration}ms",
            requestId, attempt, fileResponse.StatusCode, contentLength, fileDownloadDuration.TotalMilliseconds);

      fileResponse.EnsureSuccessStatusCode();
         diagnostics.Append($"File response received (ContentLength: {contentLength}). ");

          await using var fileStream = new FileStream(tmpFileName, FileMode.Create);
  await fileResponse.Content.CopyToAsync(fileStream);
    fileStream.Close();

      var actualFileSize = new FileInfo(tmpFileName).Length;
           var disposition = fileResponse.Content.Headers.ContentDisposition?.FileName;
var filename = disposition?.Trim('"') ?? "file";
             var fi = new FileInfo(filename);

          string newFileName;
           if (category == "Financial Report")
          {
    newFileName = ReplaceInvalidChars(
             $"{fullGrantNumber.Remove(0, 4)}-{documentName}-{Convert.ToDateTime(documentDate):MM-dd-yyyy}-{Path.GetFileNameWithoutExtension(fi.Name)}{fi.Extension}",
     "_");
            }
          else
          {
  newFileName = ReplaceInvalidChars(
     $"{fullGrantNumber.Remove(0, 4)}-{documentName}-{documentId}{fi.Extension}",
 "_");
           }

        System.IO.File.Move(tmpFileName, Path.Combine(downloadDirectory, newFileName));
       downloadData.FileDownloaded = newFileName;

               var totalDuration = DateTime.UtcNow - startTime;
 diagnostics.Append($"File saved as {newFileName} (size: {actualFileSize} bytes). ");

    Log.Information("ERA Download SUCCESS - RequestId: {RequestId}, DocumentId: {DocumentId}, FileName: {FileName}, FileSize: {FileSize}, TotalDuration: {Duration}ms, Attempts: {Attempts}",
         requestId, documentId, newFileName, actualFileSize, totalDuration.TotalMilliseconds, attempt);

            return true;
      }
   else
              {
       Log.Warning("ERA Download FAILED - RequestId: {RequestId}, Reason: Certificate not found", requestId);
   return false;
    }
           }
       catch (HttpRequestException ex) when (ex.InnerException is System.IO.IOException ||
     ex.InnerException is System.Net.Sockets.SocketException)
              {
  // Connection was forcibly closed - retry with exponential backoff
     lastException = ex;
     var attemptDuration = DateTime.UtcNow - attemptStartTime;

            Log.Warning(ex, "ERA Connection Error - RequestId: {RequestId}, Attempt: {Attempt}/{MaxRetries}, Duration: {Duration}ms, " +
                  "ExceptionType: {ExceptionType}, InnerExceptionType: {InnerType}, Message: {Message}, " +
           "DocumentId: {DocumentId}, Category: {Category}",
         requestId, attempt, maxRetries, attemptDuration.TotalMilliseconds,
           ex.GetType().Name, ex.InnerException?.GetType().Name, ex.Message,
                 documentId, category);

           diagnostics.Append($"Connection error on attempt {attempt} after {attemptDuration.TotalMilliseconds}ms: {ex.Message}. ");

         if (attempt < maxRetries)
    {
           // Exponential backoff: 2s, 4s, 8s - gives server time to recover
     var delaySeconds = Math.Pow(2, attempt);
diagnostics.Append($"Waiting {delaySeconds}s before retry. ");
   await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
       }
            }
         catch (System.IO.IOException ioEx)
     {
              lastException = ioEx;
       var attemptDuration = DateTime.UtcNow - attemptStartTime;

                  Log.Warning(ioEx, "ERA IO Error - RequestId: {RequestId}, Attempt: {Attempt}/{MaxRetries}, Duration: {Duration}ms, " +
    "Message: {Message}, DocumentId: {DocumentId}",
      requestId, attempt, maxRetries, attemptDuration.TotalMilliseconds, ioEx.Message, documentId);

  diagnostics.Append($"IO error on attempt {attempt}: {ioEx.Message}. ");

   if (attempt < maxRetries)
    {
          await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
    }
               }
catch (TaskCanceledException tcEx)
          {
       lastException = tcEx;
             var attemptDuration = DateTime.UtcNow - attemptStartTime;

              Log.Warning(tcEx, "ERA Timeout - RequestId: {RequestId}, Attempt: {Attempt}/{MaxRetries}, Duration: {Duration}ms, " +
      "DocumentId: {DocumentId}, Category: {Category}",
       requestId, attempt, maxRetries, attemptDuration.TotalMilliseconds, documentId, category);

  diagnostics.Append($"TIMEOUT on attempt {attempt} after {attemptDuration.TotalMilliseconds}ms. ");

           if (attempt < maxRetries)
      {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
               }
  }
           catch (Exception ex)
      {
             var attemptDuration = DateTime.UtcNow - attemptStartTime;

                   Log.Error(ex, "ERA Download FAILED (non-retryable) - RequestId: {RequestId}, Attempt: {Attempt}, Duration: {Duration}ms, " +
    "ExceptionType: {ExceptionType}, Message: {Message}, DocumentId: {DocumentId}, Category: {Category}",
               requestId, attempt, attemptDuration.TotalMilliseconds, ex.GetType().Name, ex.Message, documentId, category);

          diagnostics.Append($"Error: {ex.GetType().Name}: {ex.Message}. ");
            throw;
        }
        }

     // All retries failed
                var totalFailedDuration = DateTime.UtcNow - startTime;
   if (lastException != null)
           {
        diagnostics.Append($"All {maxRetries} attempts failed after {totalFailedDuration.TotalMilliseconds}ms. ");

            Log.Error(lastException, "ERA Download FAILED (all retries exhausted) - RequestId: {RequestId}, TotalDuration: {Duration}ms, " +
                "DocumentId: {DocumentId}, Category: {Category}, LastError: {LastError}",
  requestId, totalFailedDuration.TotalMilliseconds, documentId, category, lastException.Message);

     throw lastException;
   }

          return false;
            }
            finally
        {
      // Always release the semaphore to allow other requests to proceed
 _eraConnectionSemaphore.Release();
      diagnostics.Append("Connection slot released. ");
      }
        }

        /// <summary>
        /// Handle closeout notification files
        /// </summary>
        private async Task<bool> HandleCloseoutNotificationAsync(
      string category,
            string appl,
            string documentName,
            string tmpFileName,
       string downloadDirectory,
            string fullGrantNumber,
      string documentDate,
DownloadData downloadData,
   System.Text.StringBuilder diagnostics,
       SessionInfo sessionInfo)
        {
            diagnostics.Append("Closeout or FFR_Rej. ");

     // Get notification data
            var notification = await GetCloseoutNotificationAsync(appl, documentName, sessionInfo);

 if (notification != null)
  {
      diagnostics.Append("Got notification. ");
          diagnostics.Append($"Created report {appl}. ");
                byte[] bytes = GenerateCloseoutNotificationPdf(notification, appl);

                string newFileName;
                if (category == "CloseoutNotification")
        {
      newFileName = ReplaceInvalidChars(
            $"{fullGrantNumber.Remove(0, 4)}-{category}-{documentName}-{Convert.ToDateTime(documentDate):MM-dd-yyyy}.pdf",
     "_");
        }
          else
       {
 newFileName = ReplaceInvalidChars(
   $"{fullGrantNumber.Remove(0, 4)}-{documentName}-{Convert.ToDateTime(documentDate):MM-dd-yyyy}.pdf",
         "_");
   }

   await System.IO.File.WriteAllBytesAsync(tmpFileName, bytes);
       diagnostics.Append($"Wrote file to {tmpFileName} ");
                System.IO.File.Move(tmpFileName, Path.Combine(downloadDirectory, newFileName));
      diagnostics.Append("Moved.");
     downloadData.FileDownloaded = newFileName;
    return true;
    }
            else
          {
 Log.Warning("Notification not found - Check certificate at:" + sessionInfo.CertPath);
 return false;
   }
        }

        /// <summary>
        /// Generate PDF from closeout notification HTML using EmailConcatenation.PdfConverter
        /// </summary>
        private byte[] GenerateCloseoutNotificationPdf(Notification notification, string applId)
        {
            var htmlContent = $@"<!DOCTYPE html>
<html>
<head>
    <meta name=""viewport"" content=""width=device-width"" />
  <title>Closeout Notification</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 0; }}
        header {{ padding: 0 20px; }}
        h4 {{ margin: 10px 0; }}
        label {{ color: #666; }}
        .field {{ font-weight: bold; margin: 5px 0; }}
        .field label {{ width: 75px; text-align: left; display: inline-block; font-size: 0.9em; }}
        .subject-label {{ width: 75px; text-align: right; display: inline-block; color: #666; font-size: 0.9em; text-transform: uppercase; margin-top: 20px; }}
      article {{ padding: 10px 20px; }}
    </style>
</head>
<body>
    <header>
        <h4>
  <label>Grant Application Id:</label>{System.Web.HttpUtility.HtmlEncode(applId)}<br />
       <label>Notification Name:</label> {System.Web.HttpUtility.HtmlEncode(notification.notificationName ?? "")}
        </h4>
        <div class=""field""><label>From:</label> <span>{System.Web.HttpUtility.HtmlEncode(notification.fromAddress ?? "")}</span></div>
        <div class=""field""><label>To:</label> <span>{System.Web.HttpUtility.HtmlEncode(notification.toAddress ?? "")}</span></div>
        <div class=""field""><label>cc:</label> <span>{System.Web.HttpUtility.HtmlEncode(notification.ccAddress ?? "")}</span></div>
        <div class=""field""><label>Sent:</label> <span>{System.Web.HttpUtility.HtmlEncode(notification.sentDate ?? "")}</span></div>
  <div class=""field""><label class=""subject-label"">Subject:</label><span>{System.Web.HttpUtility.HtmlEncode(notification.subject ?? "")}</span></div>
    </header>
    <article id=""mailbody"">{notification.emailContent ?? ""}</article>
</body>
</html>";

         var converter = new EmailConcatenation.PdfConverter();
            var htmlBytes = System.Text.Encoding.UTF8.GetBytes(htmlContent);

            using (var memoryStream = new MemoryStream(htmlBytes))
    {
       var pdfDocument = converter.Convert(memoryStream, "closeout-notification.html");
      if (pdfDocument != null)
              {
      return pdfDocument.BinaryData;
            }
   }

            return Array.Empty<byte>();
      }

        /// <summary>
        /// Handle standard file download with SiteMinder authentication
 /// </summary>
        private async Task HandleStandardFileAsync(
         Uri uri,
            string tmpFileName,
 string downloadDirectory,
            string fullGrantNumber,
            string documentName,
          string documentId,
            DownloadData downloadData,
      System.Text.StringBuilder diagnostics,
            SessionInfo sessionInfo)
   {
            diagnostics.Append("Not closeout or FFR Rejection. ");

  var handler = new HttpClientHandler
    {
                UseDefaultCredentials = true,
 Credentials = System.Net.CredentialCache.DefaultNetworkCredentials,
  UseCookies = true,
   CookieContainer = new System.Net.CookieContainer()
     };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "eGrants");

   if (!string.IsNullOrEmpty(sessionInfo.BrowserCookies))
    {
         var cookies = sessionInfo.BrowserCookies.Split(';');
         foreach (var cookie in cookies)
   {
      var trimmedCookie = cookie.Trim();
   if (string.IsNullOrEmpty(trimmedCookie)) continue;

     var parts = trimmedCookie.Split('=', 2);
   if (parts.Length == 2)
           {
      try
        {
         handler.CookieContainer.Add(uri, new System.Net.Cookie(parts[0].Trim(), parts[1].Trim()));
   }
           catch (Exception ex)
     {
    Log.Warning(ex, "Failed to add cookie: {Cookie}", trimmedCookie);
  }
    }
    }
  }

      var response = await client.GetAsync(uri);
    response.EnsureSuccessStatusCode();

            await using var fileStream = new FileStream(tmpFileName, FileMode.Create);
      await response.Content.CopyToAsync(fileStream);
        fileStream.Close();

         var filename = Path.GetFileName(uri.LocalPath);
      var fi = new FileInfo(filename);
var newFileName = ReplaceInvalidChars($"{fullGrantNumber.Remove(0, 4)}-{documentName}-{documentId}{fi.Extension}", "_");

            System.IO.File.Move(tmpFileName, Path.Combine(downloadDirectory, newFileName), true);
            downloadData.FileDownloaded = newFileName;
        }

 /// <summary>
        /// Get closeout notification data
        /// </summary>
        public async Task<Notification> GetCloseoutNotificationAsync(string applid, string notifName, SessionInfo sessionInfo)
        {
            var cerUri = sessionInfo.CertPath;
  var certPass = sessionInfo.CertPass;
         var notif = new Notification();

   if (string.IsNullOrEmpty(cerUri) || !System.IO.File.Exists(cerUri))
            {
       Log.Warning("Certificate not found at path: {CertPath}", cerUri);
                return null;
            }

 // Load certificate with proper key storage flags for ASP.NET Core / .NET 8
            var certificate = new X509Certificate2(
  cerUri,
   certPass,
    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            var eraUrl = sessionInfo.EraUrlBase;

            const int maxRetries = 3;
    int attempt = 0;
  Exception lastException = null;

  while (attempt < maxRetries)
            {
attempt++;
             try
              {
   // Use SocketsHttpHandler for better TLS control in .NET 8
          var handler = new SocketsHttpHandler
    {
         SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                   {
            EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
  ClientCertificates = new X509Certificate2Collection { certificate },
          RemoteCertificateValidationCallback = (message, cert, chain, sslPolicyErrors) => true
               }
           };

          using var client = new HttpClient(handler);
              client.Timeout = TimeSpan.FromMinutes(5);

            var escapedApplid = SecurityElement.Escape(applid);
 var soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>  
<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:mes=""http://era.nih.gov/grantDocumentInfo/message""> 
    <soap:Header/> 
    <soap:Body>
        <mes:GrantCorrespondenceRequest><mes:applId>{escapedApplid}</mes:applId></mes:GrantCorrespondenceRequest> 
    </soap:Body>
</soap:Envelope>";

   var content = new StringContent(soapRequest, System.Text.Encoding.UTF8, "application/xml");
                var response = await client.PostAsync($"{eraUrl}grantfolder/services/GrantDocumentInfo", content);
     response.EnsureSuccessStatusCode();

     var serviceResult = await response.Content.ReadAsStringAsync();
    var pos = serviceResult.IndexOf("apache.org>") + "apache.org>".Length;
       serviceResult = serviceResult.Substring(pos);
          pos = serviceResult.IndexOf("--uuid:");
            serviceResult = serviceResult.Substring(0, pos);

           var doc = XDocument.Parse(serviceResult);
          XNamespace ns2 = "http://era.nih.gov/grantDocumentInfo/domain";
          var responses = doc.Descendants(ns2 + "correspondenceData");

          foreach (var resp in responses)
          {
    var notif_name = (string)resp.Element(ns2 + "notificationName");
             if (notif_name?.ToLower() == notifName.ToLower())
      {
    notif.notificationName = notif_name;
        notif.description = (string)resp.Element(ns2 + "description");
         notif.sentDate = (string)resp.Element(ns2 + "sentDate");
notif.fromAddress = (string)resp.Element(ns2 + "fromAddress");
                 notif.toAddress = (string)resp.Element(ns2 + "toAddress");
     notif.ccAddress = (string)resp.Element(ns2 + "ccAddress");
    notif.subject = (string)resp.Element(ns2 + "subject");
      notif.emailContent = (string)resp.Element(ns2 + "emailContent");
        break;
    }
         }
        return notif;
    }
            catch (HttpRequestException ex) when (ex.InnerException is System.IO.IOException || ex.InnerException is System.Net.Sockets.SocketException)
      {
     lastException = ex;
           if (attempt < maxRetries) await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
    }
   catch (Exception ex)
                {
Log.Error(ex, "Error getting closeout notification");
   throw;
       }
   }

    if (lastException != null)
    {
    Log.Error(lastException, "Failed to get closeout notification after {MaxRetries} attempts", maxRetries);
      }
            return null;
        }

        /// <summary>
  /// Replace invalid file name characters
        /// </summary>
        private string ReplaceInvalidChars(string filename, string replacementCharacter)
     {
     return string.Join(replacementCharacter, filename.Split(Path.GetInvalidFileNameChars()));
        }

        /// <summary>
 /// Create URI from url string
        /// </summary>
     private Uri CreateUri(string url, string imageServerUrl, System.Text.StringBuilder diagnostics)
        {
   diagnostics.Append($"Creating w/ this url : {url} ");
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
          var imageServer = new Uri(imageServerUrl);
           diagnostics.Append($"image server : {imageServer} ");
    uri = new Uri(imageServer, url);
         diagnostics.Append("Created img server uri. ");
            }
return uri;
  }

        public async Task<List<DocAttachment>> LoadDocAttachmentsAsync(int document_id)
     {
            var list = new List<DocAttachment>();
            try
            {
        await using var conn = new SqlConnection(_context.Database.GetConnectionString());
       await using var cmd = new SqlCommand("SELECT url, document_name FROM vw_attachments WHERE document_id=@document_id", conn);
    cmd.CommandType = CommandType.Text;
              cmd.Parameters.Add("@document_id", SqlDbType.Int).Value = document_id;
       await conn.OpenAsync();

        await using var rdr = await cmd.ExecuteReaderAsync();
while (await rdr.ReadAsync())
  {
           list.Add(new DocAttachment
             {
        document_name = rdr["document_name"]?.ToString(),
      url = rdr["url"]?.ToString()
  });
              }
 }
            catch (Exception ex)
  {
    Log.Error(ex, "Error loading document attachments for document_id: {DocumentId}", document_id);
            throw;
            }
       return list;
    }

   public async Task report_doc_error(string errormsg, int docId, string ic, string userId)
        {
        try
       {
    await Task.Run(() => _documentRepository.report_doc_error(errormsg, docId, ic, userId));
     Log.Information("Document error reported successfully. DocId={DocId}, User={UserId}", docId, userId);
        }
   catch (Exception ex)
            {
        Log.Error(ex, "Error reporting document error. DocId={DocId}, User={UserId}", docId, userId);
    throw;
            }
        }

  public async Task<List<DocumentInformation>> GetDocInfo(int docId)
        {
            return await _documentRepository.GetDocInfo(docId);
        }
    }
}
