using System.Data;
using System.Text;

using eGrants.DAL;
using eGrants.Models;
using eGrants.Repositories.Interfaces;
using eGrants.Services.Interfaces;
using eGrants.ViewModels;

using IronPdf;

using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using MsgReader.Outlook;

using Serilog;

namespace eGrants.Services
{
    /// <summary>
    /// Service for eGrants Funding operations
    /// </summary>
    public class EgrantsFundingService : IEgrantsFundingService
    {
        private readonly AppDbContext _context;
        private readonly ICommonRepository _commonRepository;
        private readonly ILogger<EgrantsFundingService> _logger;

        public EgrantsFundingService(
            AppDbContext context,
            ICommonRepository commonRepository,
            ILogger<EgrantsFundingService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _commonRepository = commonRepository ?? throw new ArgumentNullException(nameof(commonRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<FundingCategories>> LoadFundingCategoriesAsync(int fiscalYear)
        {
            var list = new List<FundingCategories>();

            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await using var cmd = new SqlCommand(
                    "SELECT level_id, ISNULL(parent_id,0) as parent_id,category_id,category_name,category_fy, " +
                    "dbo.fn_funding_child_count(category_id,@fy) as child_count, " +
                    "dbo.fn_funding_doc_count(category_id,@fy) as doc_count " +
                    "FROM vw_funding_categories WHERE category_fy is null or category_fy = @fy " +
                    "ORDER BY category_name",
                    conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@fy", SqlDbType.Int).Value = fiscalYear;

                await conn.OpenAsync();
                await using var rdr = await cmd.ExecuteReaderAsync();

                while (await rdr.ReadAsync())
                {
                    list.Add(new FundingCategories
                    {
                        level_id = rdr["level_id"]?.ToString(),
                        parent_id = rdr["parent_id"]?.ToString(),
                        category_id = rdr["category_id"]?.ToString(),
                        category_name = rdr["category_name"]?.ToString(),
                        child_count = rdr["child_count"]?.ToString(),
                        doc_count = rdr["doc_count"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading funding categories for fiscal year: {FiscalYear}", fiscalYear);
                throw;
            }

            return list;
        }

        public async Task<List<FundingDocuments>> LoadFundingDocsAsync(string act, int serialNum, int fiscalYear, string ic, string userId)
        {
            var list = new List<FundingDocuments>();

            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await using var cmd = new SqlCommand("sp_web_egrants_funding_docs", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
                cmd.Parameters.Add("@serial_num", SqlDbType.Int).Value = serialNum;
                cmd.Parameters.Add("@fy", SqlDbType.Int).Value = fiscalYear;
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                cmd.Parameters.Add("@Operator", SqlDbType.VarChar).Value = userId;

                await conn.OpenAsync();
                await using var rdr = await cmd.ExecuteReaderAsync();

                while (await rdr.ReadAsync())
                {
                    list.Add(new FundingDocuments
                    {
                        document_id = rdr["document_id"]?.ToString(),
                        doc_label = rdr["doc_label"]?.ToString(),
                        category_id = rdr["category_id"]?.ToString(),
                        category_name = rdr["category_name"]?.ToString(),
                        document_fy = rdr["document_fy"]?.ToString(),
                        url = rdr["url"]?.ToString(),
                        created_date = rdr["created_date"]?.ToString(),
                        arra_flag = rdr["arra_flag"]?.ToString(),
                        serial_num = rdr["serial_num"]?.ToString(),
                        appl_id = rdr["appl_id"]?.ToString(),
                        full_grant_num = rdr["full_grant_num"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading funding docs with act={Act}, serialNum={SerialNum}, fy={FiscalYear}",
                    act, serialNum, fiscalYear);
                throw;
            }

            return list;
        }

        public async Task<int> GetFundingDocIDAsync(int applId, int categoryId, DateTime docDate, string subCategory, string fileType, string ic, string userId)
        {
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await using var cmd = new SqlCommand("sp_web_egrants_funding_doc_create", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ApplID", SqlDbType.Int).Value = applId;
                cmd.Parameters.Add("@CategoryID", SqlDbType.Int).Value = categoryId;
                cmd.Parameters.Add("@DocDate", SqlDbType.DateTime).Value = docDate;
                cmd.Parameters.Add("@SubCategory", SqlDbType.VarChar).Value = subCategory ?? string.Empty;
                cmd.Parameters.Add("@FileType", SqlDbType.VarChar).Value = fileType;
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userId;

                var documentIdParam = cmd.Parameters.Add("@DocumentID", SqlDbType.Int);
                documentIdParam.Direction = ParameterDirection.Output;

                await conn.OpenAsync();
                await using var rdr = await cmd.ExecuteReaderAsync();
                await rdr.CloseAsync();

                return Convert.ToInt32(documentIdParam.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating funding document for applId={ApplId}, categoryId={CategoryId}",
                    applId, categoryId);
                throw;
            }
        }

        public async Task<List<FundingCategories>> LoadFundingCategoryListAsync()
        {
            var list = new List<FundingCategories>();

            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await using var cmd = new SqlCommand(
                    "SELECT distinct category_id,category_name,level_id,parent_id FROM funding_categories " +
                    "WHERE category_fy is null or category_fy = 2014 Order by level_id, category_name",
                    conn);

                cmd.CommandType = CommandType.Text;
                await conn.OpenAsync();
                await using var rdr = await cmd.ExecuteReaderAsync();

                while (await rdr.ReadAsync())
                {
                    list.Add(new FundingCategories
                    {
                        category_id = rdr["category_id"]?.ToString(),
                        category_name = rdr["category_name"]?.ToString(),
                        level_id = rdr["level_id"]?.ToString(),
                        parent_id = rdr["parent_id"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading funding category list");
                throw;
            }

            return list;
        }

        public async Task<int> GetMaxCategoryIdAsync(int fiscalYear)
        {
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await using var cmd = new SqlCommand(
                    "SELECT max(category_id) as max_categoryid FROM funding_categories " +
                    "WHERE category_fy is null or category_fy= @fy",
                    conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@fy", SqlDbType.Int).Value = fiscalYear;

                await conn.OpenAsync();
                await using var rdr = await cmd.ExecuteReaderAsync();

                int maxCategoryId = 0;
                if (await rdr.ReadAsync())
                {
                    maxCategoryId = Convert.ToInt32(rdr["max_categoryid"] ?? 0);
                }

                return maxCategoryId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting max category id for fiscal year: {FiscalYear}", fiscalYear);
                throw;
            }
        }

        public async Task<List<Appls>> LoadDocApplsAsync(int docId)
        {
            var list = new List<Appls>();

            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await using var cmd = new SqlCommand(
                    "SELECT distinct appl.appl_id, appl.support_year,appl.full_grant_num " +
                    "FROM vw_appls as appl, vw_funding f " +
                    "WHERE f.appl_id = appl.appl_id and f.document_id = @doc_id and f.disabled_date is null",
                    conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = docId;

                await conn.OpenAsync();
                await using var rdr = await cmd.ExecuteReaderAsync();

                while (await rdr.ReadAsync())
                {
                    list.Add(new Appls
                    {
                        appl_id = rdr["appl_id"]?.ToString(),
                        support_year = rdr["support_year"]?.ToString(),
                        full_grant_num = rdr["full_grant_num"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading doc appls for docId={DocId}", docId);
                throw;
            }

            return list;
        }

        public async Task<List<Appls>> LoadFullGrantNumbersAsync(int serialNum, string adminCode, int docId)
        {
            var list = new List<Appls>();

            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await using var cmd = new SqlCommand(
                    "SELECT appl_id, support_year, full_grant_num FROM vw_appls " +
                    "WHERE admin_phs_org_code = @admin_code and serial_num = @serial_num and " +
                    "appl_id not in (SELECT appl_id FROM funding_appls WHERE document_id = @doc_id ) " +
                    "order by support_year desc",
                    conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@serial_num", SqlDbType.Int).Value = serialNum;
                cmd.Parameters.Add("@admin_code", SqlDbType.VarChar).Value = adminCode;
                cmd.Parameters.Add("@doc_id", SqlDbType.Int).Value = docId;

                await conn.OpenAsync();
                await using var rdr = await cmd.ExecuteReaderAsync();

                while (await rdr.ReadAsync())
                {
                    list.Add(new Appls
                    {
                        appl_id = rdr["appl_id"]?.ToString(),
                        support_year = rdr["support_year"]?.ToString(),
                        full_grant_num = rdr["full_grant_num"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading full grant numbers for serialNum={SerialNum}, adminCode={AdminCode}, docId={DocId}",
                    serialNum, adminCode, docId);
                throw;
            }

            return list;
        }

        public async Task EditFundingDocAsync(string act, int applId, int docId, string ic, string userId)
        {
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await using var cmd = new SqlCommand("sp_web_egrants_funding_doc_edit", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
                cmd.Parameters.Add("@appl_id", SqlDbType.Int).Value = applId;
                cmd.Parameters.Add("@document_id", SqlDbType.Int).Value = docId;
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                cmd.Parameters.Add("@Operator", SqlDbType.VarChar).Value = userId;

                await conn.OpenAsync();
                await using var rdr = await cmd.ExecuteReaderAsync();
                await rdr.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing funding doc with act={Act}, applId={ApplId}, docId={DocId}",
                    act, applId, docId);
                throw;
            }
        }

        public async Task EditFundingApplAsync(string act, int applId, int docId, string ic, string userId)
        {
            try
            {
                await using var conn = new SqlConnection(_context.Database.GetConnectionString());
                await using var cmd = new SqlCommand("sp_web_egrants_funding_appl_edit", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
                cmd.Parameters.Add("@appl_id", SqlDbType.Int).Value = applId;
                cmd.Parameters.Add("@document_id", SqlDbType.Int).Value = docId;
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                cmd.Parameters.Add("@Operator", SqlDbType.VarChar).Value = userId;

                await conn.OpenAsync();
                await using var rdr = await cmd.ExecuteReaderAsync();
                await rdr.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing funding appl with act={Act}, applId={ApplId}, docId={DocId}",
                    act, applId, docId);
                throw;
            }
        }

        public async Task<FundingDocumentResult> CreateFundingDocByDdropAsync(
            IFormFile file,
            int applId,
            int categoryId,
            DateTime documentDate,
            string subCategory,
            SessionInfo sessionInfo)
        {
            var result = new FundingDocumentResult();

            if (file == null || file.Length == 0)
            {
                result.Success = false;
                result.Message = "You have not specified a file.";
                return result;
            }

            try
            {
                var fileName = Path.GetFileName(file.FileName);
                var fileExtension = Path.GetExtension(fileName);

                var documentId = await GetFundingDocIDAsync(
                    applId,
                    categoryId,
                    documentDate,
                    subCategory,
                    fileExtension,
                    sessionInfo.Ic,
                    sessionInfo.UserId);

                var docName = documentId > 9999
                    ? $"0{documentId}{fileExtension}"
                    : $"00{documentId}{fileExtension}";

                var fileFolder = $@"\\{sessionInfo.WebGrantUrl}\egrants\funded\nci\funding\upload\";
                var filePath = Path.Combine(fileFolder, docName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var fundingRelativePath = "egrants/funded/nci/funding/upload/";
                result.Url = $"{sessionInfo.ImageServerUrl}data/{fundingRelativePath}{docName}";
                result.Message = "Done! Funding document has been uploaded";
                result.Success = true;
                result.DocumentId = documentId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating funding doc by drag-drop for applId={ApplId}", applId);
                result.Success = false;
                result.Message = $"ERROR: {ex.Message}";
            }

            return result;
        }

        public async Task<FundingDocumentResult> CreateFundingDocByFileAsync(
            IFormFile file,
            int applId,
            int categoryId,
            DateTime documentDate,
            string subCategory,
            SessionInfo sessionInfo)
        {
            return await CreateFundingDocByDdropAsync(file, applId, categoryId, documentDate, subCategory, sessionInfo);
        }

        public async Task<FundingDocumentResult> CreateFundingPdfByFilesAsync(
            IEnumerable<IFormFile> files,
            int applId,
            int categoryId,
            DateTime documentDate,
            string subCategory,
            SessionInfo sessionInfo)
        {
            var result = new FundingDocumentResult();

            if (files == null || !files.Any())
            {
                result.Success = false;
                result.Message = "You have not specified any files.";
                return result;
            }

            try
            {
                var pdfDocs = new List<PdfDocument>();
                var converter = new EmailConcatenation.PdfConverter();
                var unsupportedFilesList = new List<string>();

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var fileExtension = Path.GetExtension(fileName);

                    using var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    PdfDocument pdfResult = null;

                    if (fileExtension.Equals(".msg", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var emailFile = new Storage.Message(memoryStream);
                        pdfResult = converter.Convert(emailFile);
                    }
                    else
                    {
                        pdfResult = converter.Convert(memoryStream, fileName);
                    }

                    if (pdfResult != null)
                    {
                        pdfDocs.Add(pdfResult);
                    }
                    else
                    {
                        unsupportedFilesList.Add(fileName);
                    }
                }

                var sb = new StringBuilder();

                if (pdfDocs.Any())
                {
                    var documentId = await GetFundingDocIDAsync(
                        applId,
                        categoryId,
                        documentDate,
                        subCategory,
                        ".pdf",
                        sessionInfo.Ic,
                        sessionInfo.UserId);

                    var docName = documentId > 9999
                        ? $"0{documentId}.pdf"
                        : $"00{documentId}.pdf";

                    var fileFolder = $@"\\{sessionInfo.WebGrantUrl}\egrants\funded\nci\funding\upload\";
                    var filePath = Path.Combine(fileFolder, docName);

                    var mergedPdf = PdfDocument.Merge(pdfDocs);
                    mergedPdf.SaveAs(filePath);

                    var fundingRelativePath = "egrants/funded/nci/funding/upload/";
                    result.Url = $"{sessionInfo.ImageServerUrl}data/{fundingRelativePath}{docName}";
                    result.DocumentId = documentId;
                    sb.Append("Done! New document has been created**#7|n3br3@k#**");
                }
                else
                {
                    sb.Append("No documents were found to convert**#7|n3br3@k#**");
                }

                if (unsupportedFilesList.Count > 0)
                {
                    sb.AppendLine("IMPORTANT! The following email attachments were not converted, please add them separately: **#h3@d3r#****#7|n3br3@k#**");
                    foreach (var unsupportedFile in unsupportedFilesList)
                    {
                        sb.AppendLine($"{unsupportedFile}**#7|n3br3@k#**");
                    }
                }

                result.Success = true;
                result.Message = sb.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PDF funding doc for applId={ApplId}", applId);
                result.Success = false;
                result.Message = "ERROR: The file could not be converted!";
            }

            return result;
        }
    }
}