#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  EgrantsController.cs
// Solution: eGrants
// Project:  eGrants
// Created: 2025-08-01
// Contributors:
//      - Dehuff, Daryl (NIH/NCI) [C] - dehuffdc
// Copyright (c) National Institute of Health
// 
// <Description of the file>
// 
// This source is subject to the NIH Softwre License.
// See https://ncihub.org/resources/899/download/Guidelines_for_Releasing_Research_Software_04062015.pdf
// All other rights reserved.
// 
// THE SOFTWARE IS PROVIDED "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT ARE DISCLAIMED. IN NO EVENT SHALL THE NATIONAL
// CANCER INSTITUTE (THE PROVIDER), THE NATIONAL INSTITUTES OF HEALTH, THE
// U.S. GOVERNMENT OR THE INDIVIDUAL DEVELOPERS BE LIABLE FOR ANY DIRECT,
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
// \***************************************************************************/

#endregion

#region

using eGrants.Models;
using eGrants.Services.Interfaces;
using eGrants.ViewModels;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

#endregion
namespace eGrants.Controllers.Egrants
{
    /// <summary>
    /// The egrants controller.
    /// </summary>
    public class EgrantsController : Controller
    {
        const int MAX_RETRIES = 3;
        // Injected dependencies: database context and product service

        private readonly IeGrantsService _eGrantsService;
        private readonly IDocumentService _documentService;
        private readonly ICommonService _commonService;
        private readonly ISessionInfoService _sessionInfoService;

        public EgrantsController(IeGrantsService eGrantsService, ICommonService commonService, IDocumentService documentService, ISessionInfoService sessionInfoService)
        {
            _eGrantsService = eGrantsService;
            _commonService = commonService;
            _sessionInfoService = sessionInfoService;
            _documentService = documentService;
        }

        // go to default 
        /// <summary>
        /// The go_to_default.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.CIS
        /// </returns>
        public ActionResult Go_to_default()
        {
            return View("~/Views/Shared/Go_to_Default.cshtml");
        }

        // GET: Egrants
        /// <summary>   
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<IActionResult> Index()
        {
            // May want to move this to a base controller, an action filter, or use a shared service in the long term.

            eGrantsSearchViewModel eGrantsSearchViewModelList = new eGrantsSearchViewModel();

            eGrantsSearchViewModelList.ICList = await _commonService.LoadAdminCodes();

            return View("~/Views/Index.cshtml", eGrantsSearchViewModelList);
        }

        //    public string SetCurrentViewSessionVariable(string currentView)
        //    {
        //        Console.WriteLine("In setting session Variable: " + currentView);
        //        Session["CurrentView"] = currentView;

        //        return currentView;
        //    }

        //    /// <summary>
        //    /// HttpPost
        //    /// Download the files to the temp directory from the links checked on the page. Then return the stream of bytes to the calling method.
        //    /// </summary>
        //    /// <param name="appl"></param>
        //    /// <param name="listOfUrl"></param>
        //    /// <returns></returns>
        //    public ActionResult IsDownloadForm(string appl, string fullGrantNumber, IList<string> listOfUrl)
        //    {
        //        // 1 - trim the first character in the full grant number
        //        // 2 - trim the characters in full grant number year, and anything after trim
        //        string downloadDirectory;

        //        // The downloadModel contains all of the data that will be returned to the view
        //        DownloadModel downloadModel = new DownloadModel();

        //        try
        //        {

        //            downloadModel.ApplId = appl;
        //            downloadModel.NumFailed = 0;
        //            downloadModel.NumSucceeded = 0;
        //            downloadModel.NumToDownload = listOfUrl.Count();

        //            // create the temp path and
        //            downloadDirectory = Path.Combine(Path.GetTempPath(), appl);

        //            // create or return an existing directory to hold the downloaded files
        //            DirectoryInfo directoryInfo = Directory.CreateDirectory(downloadDirectory);

        //            // delete all the files in this directory if there are any
        //            foreach (FileInfo file in directoryInfo.GetFiles())
        //            {
        //                file.Delete();
        //            }

        //            // delete all the folders in this directory if there are any
        //            foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
        //            {
        //                dir.Delete(true);
        //            }
        //        }
        //        catch (ArgumentNullException ex)
        //        {
        //            downloadModel.Error = "There are no URLs in the list!";
        //            return Json(downloadModel, JsonRequestBehavior.AllowGet);
        //        }
        //        catch (Exception ex)
        //        {
        //            downloadModel.Error = "General Exception. This is likely an error in accessing temp files and temp directories! Notify Development Team of this error.";
        //            return Json(downloadModel, JsonRequestBehavior.AllowGet);
        //        }

        //        // var grantId = this.ViewBag.GrantID;
        //        DownloadData downloadData = new DownloadData();
        //        downloadModel.DownloadDataList = new List<DownloadData>();

        //        // obtain the document url from the remote system
        //        var cerUri = ConfigurationManager.ConnectionStrings["certPath"].ToString();
        //        var certPass = ConfigurationManager.ConnectionStrings["certPass"].ToString();
        //        var certificate = new X509Certificate2(cerUri, certPass);

        //        var diagnostics = new StringBuilder();

        //        foreach (var dataInput in listOfUrl)
        //        {
        //            try
        //            {
        //                downloadData = new DownloadData();

        //                var split = dataInput.Split(new char[] { '|' }, StringSplitOptions.None);

        //                var url = split[0];
        //                var category = split[1];
        //                var subCategory = split[2];
        //                var documentId = split[3];
        //                var documentName = split[4];
        //                var documentDate = split[5];


        //                downloadData.Url = url;
        //                downloadData.Category = category;
        //                downloadData.SubCategory = subCategory;
        //                downloadData.DocumentId = string.IsNullOrEmpty(documentId) ? 0 : Convert.ToInt32(documentId);
        //                downloadData.DocumentName = documentName;
        //                downloadData.DocumentDate = DateTime.TryParse(documentDate, out DateTime result) ? result : DateTime.MinValue;



        //                // if(downloadModel.DownloadDataList.)
        //                // get a temp file to save the downloaded file
        //                string tmpFileName = Path.GetTempFileName();

        //                // if this is an i2e file
        //                if (url.Contains("https://i2e"))
        //                {

        //                    Console.WriteLine("We should never hit this....");

        //                    throw new Exception("We found an i2e path and these should not be included in downloads");
        //                }

        //                // if this is a file on the ERA Server
        //                if (url.Contains("https://services."))
        //                {
        //                    diagnostics.Append("Handling as era service. ");
        //                    var uri = new Uri(url);
        //                    diagnostics.Append("Uri created. ");

        //                    // obtain the document url from the remote system
        //                    // var cerUri = ConfigurationManager.ConnectionStrings["certPath"].ToString();
        //                    // var certPass = ConfigurationManager.ConnectionStrings["certPass"].ToString();
        //                    // var certificate = new X509Certificate2(cerUri, certPass);

        //                    var webRequest = (HttpWebRequest)WebRequest.Create(uri);
        //                    webRequest.KeepAlive = false;
        //                    webRequest.Method = "GET";
        //                    webRequest.AllowAutoRedirect = false;
        //                    webRequest.ClientCertificates.Add(certificate);

        //                    var webResponse = (HttpWebResponse)webRequest.GetResponse();

        //                    using (var postStream = webResponse.GetResponseStream())
        //                    {
        //                        if (postStream == null)
        //                        {
        //                            throw new Exception("The stream was empty!");
        //                        }

        //                        string downloadUrl;

        //                        using (var reader = new StreamReader(postStream))
        //                        {
        //                            downloadUrl = reader.ReadToEnd();
        //                        }

        //                        using (var myWebClient = new MyWebClient())
        //                        {
        //                            myWebClient.Credentials = CredentialCache.DefaultCredentials;

        //                            // Download the Web resource and save it into the current filesystem folder.
        //                            myWebClient.DownloadFile(downloadUrl, tmpFileName);

        //                            // // get the filename from the content-disposition header of the downloaded file
        //                            var disposition = myWebClient.ResponseHeaders["Content-Disposition"];
        //                            ContentDisposition contentDisposition = new ContentDisposition(disposition);
        //                            string filename = contentDisposition.FileName;
        //                            FileInfo fi = new FileInfo(filename);

        //                            string newFileName = string.Empty;

        //                            if (category == "Financial Report")
        //                            {
        //                                newFileName = ReplaceInvalidChars(
        //                                    $"{fullGrantNumber.Remove(0, 4)}-{documentName}-{Convert.ToDateTime(documentDate):MM-dd-yyyy}-{Path.GetFileNameWithoutExtension(fi.Name)}{fi.Extension}", "_");
        //                            }
        //                            else
        //                            {
        //                                // just reove the first four characters which are the first digit, the P30 part, concat the document_name and the file extention
        //                                // and remove all invalid characters from filename and replace with _
        //                                newFileName = ReplaceInvalidChars($"{fullGrantNumber.Remove(0, 4)}-{documentName}-{documentId}{fi.Extension}", "_");
        //                            }

        //                            // move the file from the temp file to a file with the filename in the downloadDirectory
        //                            System.IO.File.Move(tmpFileName, Path.Combine(downloadDirectory, newFileName));
        //                            downloadData.FileDownloaded = newFileName;
        //                        }
        //                    }

        //                    downloadModel.NumSucceeded += 1;
        //                }
        //                else
        //                {
        //                    diagnostics.Append("Not era file. ");
        //                    Uri uri;
        //                    diagnostics.Append($"Creating w/ this url : {url} ");
        //                    if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
        //                    {
        //                        var imageServer = new Uri(this.Session["ImageServerUrl"].ToString());
        //                        diagnostics.Append($"image server : {imageServer} ");
        //                        uri = new Uri(imageServer, url);
        //                        diagnostics.Append("Created img server uri. ");
        //                    }
        //                    diagnostics.Append("Completed uri creation. ");

        //                    if (category == "CloseoutNotification" || category == "FFR_REJECTION")
        //                    {
        //                        diagnostics.Append("Closeout or FFR_Rej. ");
        //                        ViewBag.notification = EgrantsDoc.getCloseoutNotif(appl, documentName);
        //                        diagnostics.Append("Got notification. ");
        //                        ViewBag.applid = appl;

        //                        var report = new ViewAsPdf("~/Egrants/Views/CloseoutNotif.cshtml");
        //                        diagnostics.Append($"Created report {appl}. ");
        //                        byte[] bytes = report.BuildFile(ControllerContext);


        //                        string newFileName = string.Empty;

        //                        // just remove the first four characters which are the first digit, the P30 part, concat the document_name and the file extension
        //                        // and remove all invalid characters from filename and replace with _
        //                        if (category == "CloseoutNotification")
        //                        {
        //                            newFileName = ReplaceInvalidChars(
        //                                $"{fullGrantNumber.Remove(0, 4)}-{category}-{documentName}-{Convert.ToDateTime(documentDate):MM-dd-yyyy}.pdf", "_");
        //                        }

        //                        if (category == "FFR_REJECTION")
        //                        {
        //                            newFileName = ReplaceInvalidChars(
        //                                $"{fullGrantNumber.Remove(0, 4)}-{documentName}-{Convert.ToDateTime(documentDate):MM-dd-yyyy}.pdf", "_");
        //                        }

        //                        System.IO.File.WriteAllBytes(tmpFileName, bytes);



        //                        // move the file from the temp file to a file with the filename in the downloadDirectory
        //                        diagnostics.Append($"Wrote file to {tmpFileName} ");
        //                        System.IO.File.Move(tmpFileName, Path.Combine(downloadDirectory, newFileName));
        //                        diagnostics.Append($"Moved.");
        //                        downloadData.FileDownloaded = newFileName;
        //                    }
        //                    else
        //                    {
        //                        diagnostics.Append($"Not closeout or FFR Rejection. ");
        //                        using (var myWebClient = new WebClient())
        //                        {
        //                            myWebClient.UseDefaultCredentials = true;
        //                            myWebClient.Credentials = CredentialCache.DefaultNetworkCredentials;
        //                            myWebClient.Credentials = CredentialCache.DefaultCredentials;

        //                            myWebClient.Headers.Add(HttpRequestHeader.Cookie, Request.Headers["cookie"]);

        //                            myWebClient.DownloadFile(uri, tmpFileName);
        //                            string filename = Path.GetFileName(uri.LocalPath);
        //                            FileInfo fi = new FileInfo(filename);

        //                            string newFileName = string.Empty;

        //                            // just remove the first four characters which are the first digit, the P30 part, concat the document_name and the file extension
        //                            // and remove all invalid characters from filename and replace with _
        //                            newFileName = ReplaceInvalidChars($"{fullGrantNumber.Remove(0, 4)}-{documentName}-{documentId}{fi.Extension}", "_");

        //                            // move the file from the temp file to a file with the filename in the downloadDirectory
        //                            System.IO.File.Move(tmpFileName, Path.Combine(downloadDirectory, newFileName));
        //                            downloadData.FileDownloaded = newFileName;
        //                        }
        //                    }

        //                    downloadModel.NumSucceeded += 1;
        //                }
        //            }
        //            catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
        //            {
        //                // code specifically for a WebException NotFound
        //                downloadData.Error = "File not found.";
        //            }
        //            catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.InternalServerError)
        //            {
        //                // code specifically for a WebException InternalServerError
        //                downloadData.Error = "Internal Server Error! Notify Dev Team!";
        //            }
        //            catch (ArgumentNullException ex)
        //            {
        //                downloadModel.Error = "An value is null which should not be.";
        //            }
        //            catch (Exception err)
        //            {
        //                downloadData.Error = "General Exception! Screenshot and this message and notify the Development Team: " + Environment.NewLine + err.Message.ToString() + diagnostics.ToString();
        //                downloadModel.NumFailed += 1;
        //            }

        //            downloadModel.DownloadDataList.Add(downloadData);
        //        }

        //        string handle = Guid.NewGuid().ToString();
        //        downloadModel.Handle = handle;

        //        string zipFileName = fullGrantNumber.Remove(0,1) + ".zip";
        //        string zipFileNameWithPath = Path.Combine(Path.GetTempPath(), zipFileName);

        //        downloadModel.ZipFilename = zipFileName;

        //        try
        //        {
        //            // if the zip file exists delete it
        //            if (System.IO.File.Exists(zipFileNameWithPath))
        //            {
        //                System.IO.File.Delete(zipFileNameWithPath);
        //            }

        //            // zip the contents of the downloadDirectory to the zipPath
        //            ZipFile.CreateFromDirectory(downloadDirectory, zipFileNameWithPath);

        //            using (MemoryStream ms = new MemoryStream())
        //            using (FileStream file = new FileStream(zipFileNameWithPath, FileMode.Open, FileAccess.Read))
        //            {
        //                byte[] bytes = new byte[file.Length];
        //                file.Read(bytes, 0, (int)file.Length);
        //                ms.Write(bytes, 0, (int)file.Length);
        //                TempData[handle] = ms.ToArray();
        //            }
        //        }
        //        catch (Exception err)
        //        {
        //            Console.WriteLine("Error trying to Zip or serve zip file: " + err.ToString());
        //            downloadModel.Error = "ZIP FILE ERROR! Screen shot this error and send to Dev team! " + Environment.NewLine + err.ToString();
        //        }

        //        return Json(downloadModel, JsonRequestBehavior.AllowGet);
        //    }

        //    /// <summary>  
        //    /// Override the JSON Result with Max integer JSON lenght  
        //    /// </summary>  
        //    /// <param name="data">Data</param>  
        //    /// <param name="contentType">Content Type</param>  
        //    /// <param name="contentEncoding">Content Encoding</param>  
        //    /// <param name="behavior">Behavior</param>  
        //    /// <returns>As JsonResult</returns>  
        //    protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        //    {
        //        return new JsonResult()
        //                   {
        //                       Data = data,
        //                       ContentType = contentType,
        //                       ContentEncoding = contentEncoding,
        //                       JsonRequestBehavior = behavior,
        //                       MaxJsonLength = int.MaxValue
        //                   };
        //    }

        //    // [HttpGet]
        //    public virtual ActionResult Download(string fileGuid, string fileName)
        //    {

        //        if (TempData[fileGuid] != null)
        //        {
        //            byte[] data = TempData[fileGuid] as byte[];

        //            var cd = new ContentDisposition
        //                         {
        //                             // for example foo.bak
        //                             FileName = fileName,

        //                             // always prompt the user for downloading, set to true if you want 
        //                             // the browser to try to show the file inline
        //                             Inline = false,
        //                         };

        //            Response.AppendHeader("Content-Disposition", cd.ToString());

        //            return File(data, "application/zip");
        //        }
        //        else
        //        {
        //            // Problem - Log the error, generate a blank file,
        //            //           redirect to another controller action - whatever fits with your application
        //            return new EmptyResult();
        //        }
        //    }

        //    public string ReplaceInvalidChars(string filename, string replacementCharacter)
        //    {
        //        return string.Join(replacementCharacter, filename.Split(Path.GetInvalidFileNameChars()));
        //    }

        //    /// <summary>
        //    /// Get all appls list for appls toggle by grant_id
        //    /// </summary>
        //    /// <param name="grant_id">
        //    /// The grant_id.
        //    /// </param>
        //    /// <returns>
        //    /// The <see cref="string"/>.
        //    /// </returns>
        //    public string LoadAllAppls(int grant_id)
        //    {
        //            List<string> list = EgrantsAppl.GetAllAppls(grant_id);

        //            // JavaScriptSerializer js = new JavaScriptSerializer();
        //            return JsonConvert.SerializeObject(list);
        //    }

        //    /// <summary>
        //    /// Load 12 appls list for appls toggle by grant_id
        //    /// </summary>
        //    /// <param name="grant_id">
        //    /// The grant_id.
        //    /// </param>
        //    /// <returns>
        //    /// The <see cref="string"/>.
        //    /// </returns>
        //    public string LoadDefaultAppls(int grant_id)
        //    {
        //        var list = EgrantsAppl.GetDefaultAppls(grant_id);

        //        // JavaScriptSerializer js = new JavaScriptSerializer();
        //        return JsonConvert.SerializeObject(list);
        //    }

        //    // get appls list with documents by (admin_code and serial_num) commented out by Leon at 3/15/2019
        //    // public string LoadYears(string admin_code, string serial_num)   //string fy, string mechan, s
        //    // {
        //    // List<string> yearlist = Egrants.Models.Egrants.GetYearList(admin_code, serial_num);
        //    // JavaScriptSerializer js = new JavaScriptSerializer();
        //    // return js.Serialize(yearlist);           
        //    // }

        // get appls list with documents by (admin_code and serial_num) added by Ayu at 3/15/2019
        /// <summary>
        /// The load years.
        /// </summary>
        /// <param name="fiscalYear">
        /// The fy.
        /// </param>
        /// <param name="mechanism">
        /// The mechanism.
        /// </param>
        /// <param name="adminCode">
        /// The adminCode.
        /// </param>
        /// <param name="serialNumber">
        /// The serialNumber.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public async Task<string> LoadYears(
            string fiscalYear = null,
            string mechanism = null,
            string adminCode = null,
            string serialNumber = null)
        {
            var yearList = new List<string>();
            var list = await _eGrantsService.GetYearList(fiscalYear, mechanism, adminCode, serialNumber);

            foreach (GrantDataYears val in list)
            {
                yearList.Add(val.full_grant_num + ":" + val.appl_id);
            }

            // JavaScriptSerializer js = new JavaScriptSerializer();
            return JsonConvert.SerializeObject(yearList);
        }

        // load all appls list with or without documents
        /// <summary>
        /// The get all appls list.
        /// </summary>
        /// <param name="adminCode">
        /// The admin_code.
        /// </param>
        /// <param name="serialNum">
        /// The serial_num.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public async Task<string> GetAllApplsList(string adminCode, string serialNum)
        {
            var list = await _eGrantsService.GetAllApplsListAsync(adminCode, serialNum);

            return JsonConvert.SerializeObject(list);
        }

        // get category list by grant_id and years
        /// <summary>
        /// The load categories.
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <param name="years">
        /// The years.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public async Task<string> LoadCategories(int grantId, string years)
        {
            var list = await _eGrantsService.GetCategoryList(grantId, years);

            return JsonConvert.SerializeObject(list);
        }

        // get category list by grant_id and years
        /// <summary>
        /// The load categories.
        /// </summary>
        /// <param name="name">
        /// The new label for the grant year
        /// </param>
        /// <param name="applId">
        /// The appl_id for the grant year about to be renamed
        /// </param>
        /// <returns>
        /// The function returns true if successful<see cref="bool"/>.
        /// </returns>
        public bool NewGrantYearName(string name, int applId)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = string.Empty;
            }
            var length = name.Length;
            var truncatedName = name.Substring(0, Math.Min(length, 10));

            _eGrantsService.SetGrantYearLabel(name, applId);

            return true;
        }

        //    //public CountProperty<int> CountProperty;// = new CountProperty<int>();
        //    //countProperty.Value = 0;

        /// <summary>
        /// The by_str.
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <param name="mode">
        /// The mode.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> by_str(string str, string mode = null)
        {
            //TODO: Determine if the following code is ever being used
            // CountProperty = new CountProperty<int>();
            // CountProperty.Value = 0;
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            eGrantsSearchViewModel eGrantsSearchViewModelList = await _eGrantsService.GetEgrantsByStrAsync(str, 0, 0, 0, sessionInfo);

            eGrantsSearchViewModelList.Mode = mode;
            eGrantsSearchViewModelList.ICList = await _commonService.LoadAdminCodes();
            return View("~/Views/Index.cshtml", eGrantsSearchViewModelList);
        }

        /// <summary>
        /// The by_grant.
        /// </summary>
        /// <param name="grantId">
        /// The grant_id.
        /// </param>
        /// <param name="package">
        /// The package.
        /// </param>
        /// <param name="categories">
        /// The categories.
        /// </param>
        /// <param name="applsList">
        /// The appls_list.
        /// </param>
        /// <param name="years">
        /// The years.
        /// </param>
        /// <param name="mode">
        /// The mode.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<IActionResult> by_grant(
            int grantId = 0,
            string package = "",
            string categories = "",
            string applsList = "",
            string years = "",
            string mode = "")
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            eGrantsSearchViewModel eGrantsSearchViewModelList = await _eGrantsService.GetEgrantsByGrantAsync(string.Empty,
                grantId, package, 0, 0, categories, applsList, years, mode, sessionInfo);

            eGrantsSearchViewModelList.ICList = await _commonService.LoadAdminCodes();
            return View("~/Views/Index.cshtml", eGrantsSearchViewModelList);
        }

        /// <summary>
        /// The by_grant.
        /// </summary>
        /// <param name="grantId">
        /// The grant_id.
        /// </param>
        /// <param name="package">
        /// The package.
        /// </param>
        /// <param name="categories">
        /// The categories.
        /// </param>
        /// <param name="applsList">
        /// The appls_list.
        /// </param>
        /// <param name="years">
        /// The years.
        /// </param>
        /// <param name="mode">
        /// The mode.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<IActionResult> by_appl(
            int applId = 0,
            string mode = null,
            string str = null)
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            eGrantsSearchViewModel eGrantsSearchViewModelList = await _eGrantsService.GetEgrantsByApplAsync(applId, mode, str, sessionInfo);

            eGrantsSearchViewModelList.ICList = await _commonService.LoadAdminCodes();

            return View("~/Views/Index.cshtml", eGrantsSearchViewModelList);
        }


        //    /// <summary>
        //    /// The by_appl.
        //    /// </summary>
        //    /// <param name="appl_id">
        //    /// The appl_id.
        //    /// </param>
        //    /// <param name="mode">
        //    /// The mode.
        //    /// </param>
        //    /// <param name="str">
        //    /// The str.
        //    /// </param>
        //    /// <returns>
        //    /// The <see cref="ActionResult"/>.
        //    /// </returns>
        //    public ActionResult by_appl(int appl_id = 0, string mode = null, string str = null)
        //    {
        //        ViewBag.ICList = EgrantsCommon.LoadAdminCodes();
        //        var isexisting = EgrantsAppl.CheckApplID(appl_id);

        //        if (appl_id == 0 || isexisting == 0)
        //        {
        //            ViewBag.Message = "No data found for the search";
        //            ViewBag.grantlayer = null;
        //        }
        //        else
        //        {
        //            // ViewBag.YearList = Egrants.Models.Egrants.P2_getYearList();
        //            if (str != null)
        //                ViewBag.Str = str;


        //            ViewBag.Mode = mode;
        //            ViewBag.SearchStyle = "by_appl";
        //            ViewBag.ApplID = appl_id;
        //            ViewBag.GrantID = Dashboard.Functions.Egrants.GetGrantID(appl_id);
        //            ViewBag.SelectedCats = "All";
        //            ViewBag.SelectedCategories = "All";
        //            ViewBag.SelectedAppls = appl_id.ToString();

        //            // load data from DB
        //            Search.egrants_search(
        //                string.Empty,
        //                0,
        //                string.Empty,
        //                appl_id,
        //                0,
        //                Convert.ToString(this.Session["browser"]),
        //                Convert.ToString(this.Session["ic"]),
        //                Convert.ToString(this.Session["userid"]));

        //            ViewBag.grantlayer = Search.grantlayerproperty;
        //            ViewBag.appllayer = Search.appllayerproperty;
        //            ViewBag.appllayer_All = Search.appllayerproperty;
        //            ViewBag.ApplCount = ViewBag.appllayer.Count;
        //            ViewBag.doclayer = Search.doclayerproperty;
        //            ViewBag.DocCount = ViewBag.doclayer.Count;
        //            if (Search.appllayerproperty != null && Search.appllayerproperty.Count() > 0)
        //            {
        //                var thisAppl = Search.appllayerproperty.FirstOrDefault(a => a.appl_id == appl_id.ToString());
        //                if (thisAppl != null)
        //                    ViewBag.yearName = thisAppl.label;
        //            }
        //        }

        //        return View("~/Egrants/Views/Index.cshtml");
        //    }

        //    /// <summary>
        //    /// The by_qc.
        //    /// </summary>
        //    /// <param name="str">
        //    /// The str.
        //    /// </param>
        //    /// <returns>
        //    /// The <see cref="ActionResult"/>.
        //    /// </returns>
        //    public ActionResult by_qc(string str = null)
        //    {
        //        ViewBag.ICList = EgrantsCommon.LoadAdminCodes();

        //        // if (str == null || str == "")
        //        // {
        //        // ViewBag.Message = "No data found for the search";
        //        // ViewBag.grantlayer = null;
        //        // }
        //        // else
        //        // {
        //        ViewBag.str = "qc";
        //        ViewBag.Mode = "qc";

        //        // ViewBag.DocSort = "date";
        //        ViewBag.CurrentTab = 1;
        //        ViewBag.CurrentPage = 1;
        //        ViewBag.SearchStyle = "by_qc";

        //        // load data
        //        Search.egrants_search(
        //            "qc",
        //            0,
        //            string.Empty,
        //            0,
        //            1,
        //            Convert.ToString(this.Session["browser"]),
        //            Convert.ToString(this.Session["ic"]),
        //            Convert.ToString(this.Session["userid"]));

        //        ViewBag.grantlayer = Search.grantlayerproperty;
        //        ViewBag.appllayer = Search.appllayerproperty;
        //        ViewBag.appllayer_All = Search.appllayerproperty;
        //        ViewBag.ApplCount = ViewBag.appllayer.Count;
        //        ViewBag.doclayer = Search.doclayerproperty;
        //        ViewBag.DocCount = ViewBag.doclayer.Count;

        //        ViewBag.Pagination = Dashboard.Functions.Egrants.LoadPagination(
        //            "qc",
        //            Convert.ToString(this.Session["ic"]),
        //            Convert.ToString(this.Session["userid"]),
        //            string.Empty);

        //        ViewBag.UnidentifiedDocs = EgrantsDoc.LoadDocsUnidentified(
        //            Convert.ToString(this.Session["ImageServerUrl"]),
        //            Convert.ToString(this.Session["userid"]));



        //        return View("~/Egrants/Views/Index.cshtml");
        //    }

        /// <summary>
        /// The by_filters.
        /// </summary>
        /// <param name="fiscalYear">
        /// The fiscalYear.
        /// </param>
        /// <param name="mechanism">
        /// The mechanism.
        /// </param>
        /// <param name="adminCode">
        /// The adminCode.
        /// </param>
        /// <param name="serialNum">
        /// The serialNumber.
        /// </param>
        /// <param name="pageNum">
        /// The page number
        /// </param>
        /// <param name="tabNum">
        /// The tab number
        /// </param>
        /// <param name="packages">
        /// The package name
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<IActionResult> by_filters(int fiscalYear = 0, string mechanism = null, string adminCode = null, int serialNum = 0, int pageNum = 1, int tabNum = 1, string packages = "")
        {
            eGrantsSearchViewModel eGrantsSearchViewModelList = new eGrantsSearchViewModel();

            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            eGrantsSearchViewModelList = await _eGrantsService.GetEgrantsByFilterAsync(fiscalYear, mechanism, serialNum, adminCode, 0, 0, pageNum, sessionInfo, tabNum, packages);
            eGrantsSearchViewModelList.ICList = await _commonService.LoadAdminCodes();

            return View("~/Views/Index.cshtml", eGrantsSearchViewModelList);
        }

        //    /// <summary>
        //    /// The by_filters_page.
        //    /// </summary>
        //    /// <param name="tab_num">
        //    /// The tab_num.
        //    /// </param>
        //    /// <param name="page_num">
        //    /// The page_num.
        //    /// </param>
        //    /// <param name="package">
        //    /// The package.
        //    /// </param>
        //    /// <param name="fy">
        //    /// The fy.
        //    /// </param>
        //    /// <param name="mechanism">
        //    /// The mechanism.
        //    /// </param>
        //    /// <param name="admincode">
        //    /// The admincode.
        //    /// </param>
        //    /// <param name="serialnum">
        //    /// The serialnum.
        //    /// </param>
        //    /// <returns>
        //    /// The <see cref="ActionResult"/>.
        //    /// </returns>
        //    public ActionResult by_filters_page(
        //        int tab_num = 0,
        //        int page_num = 0,
        //        string package = null,
        //        int fiscalYear = 0,
        //        string mechanism = null,
        //        string adminCode = null,
        //        int serialNumber = 0)
        //    {
        //        ViewBag.ICList = EgrantsCommon.LoadAdminCodes();

        //        /*string.IsNullOrEmpty(admincode) &&*/
        //        if (fiscalYear == 0 && string.IsNullOrEmpty(mechanism) && serialNumber == 0)
        //        {
        //            ViewBag.Message = "No data found for the search";
        //            ViewBag.grantlayer = null;
        //        }
        //        else if (tab_num == 0 || page_num == 0 || string.IsNullOrEmpty(package) || package != "by_filters")
        //        {
        //            ViewBag.Message = "No data found for the search";
        //            ViewBag.grantlayer = null;
        //        }
        //        else
        //        {
        //            ViewBag.SearchStyle = package;
        //            ViewBag.CurrentTab = tab_num;
        //            ViewBag.CurrentPage = page_num;

        //            // create return value
        //            if (fiscalYear != 0)
        //                ViewBag.FilterFY = fiscalYear;
        //            else
        //                ViewBag.FilterFY = string.Empty;

        //            ViewBag.FilterMechanism = mechanism;
        //            ViewBag.FilterAdminCode = adminCode;

        //            if (serialNumber != 0)
        //                ViewBag.FilterSerialNumber = serialNumber;

        //            // create filters search sql query
        //            var FilterSearchQuery = Dashboard.Functions.Egrants.GetSearchQuery(
        //                fiscalYear,
        //                mechanism,
        //                adminCode,
        //                serialNumber,
        //                page_num,
        //                Convert.ToString(this.Session["browser"]),
        //                Convert.ToString(this.Session["ic"]),
        //                Convert.ToString(this.Session["userid"]));

        //            // load data
        //            Search.egrants_search(
        //                FilterSearchQuery,
        //                0,
        //                package,
        //                0,
        //                page_num,
        //                Convert.ToString(this.Session["browser"]),
        //                Convert.ToString(this.Session["ic"]),
        //                Convert.ToString(this.Session["userid"]));

        //            ViewBag.grantlayer = Search.grantlayerproperty;
        //            ViewBag.appllayer = Search.appllayerproperty;
        //            ViewBag.appllayer_All = Search.appllayerproperty;
        //            ViewBag.ApplCount = ViewBag.appllayer.Count;

        //            // show Pagination 
        //            ViewBag.Pagination = Dashboard.Functions.Egrants.LoadPagination(
        //                FilterSearchQuery,
        //                Convert.ToString(this.Session["ic"]),
        //                Convert.ToString(this.Session["userid"]),
        //                package);
        //        }

        //        return View("~/Egrants/Views/Index.cshtml");
        //    }

        /// <summary>
        /// The by_page.
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <param name="tabNum">
        /// The tab_num.
        /// </param>
        /// <param name="pageNum">
        /// The page_num.
        /// </param>
        /// <param name="package">
        /// The package.
        /// </param>
        /// <param name="mode">
        /// The mode.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<ActionResult> by_page(string str = null, int tabNum = 0, int pageNum = 0, string package = null, string mode = null)
        {
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            eGrantsSearchViewModel eGrantsSearchViewModelList = await _eGrantsService.GetEgrantsByPageAsync(str, 0, 0, pageNum, tabNum, sessionInfo, _documentService);

            eGrantsSearchViewModelList.Mode = str == "qc" ? "qc" : mode;
            eGrantsSearchViewModelList.ICList = await _commonService.LoadAdminCodes();
            return View("~/Views/Index.cshtml", eGrantsSearchViewModelList);
        }

        // Autocomplete for fy, activity_code and serial_number
        /// <summary>
        /// The load_data_autocomplete.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="term">
        /// The term.
        /// </param>
        /// <param name="mechanism">
        /// The mechanism.
        /// </param>
        /// <param name="fy">
        /// The fy.
        /// </param>
        /// <param name="admincode">
        /// The admincode.
        /// </param>
        /// <param name="serialnum">
        /// The serialnum.
        /// </param>
        /// <returns>
        /// The <see cref="JsonResult"/>.
        /// </returns>
        public async Task<JsonResult> load_data_autocomplete(
            string type,
            string term,
            string mechanism = null,
            string fy = null,
            string adminCode = null,
            string serialNum = null)
        {
            var viewModel = new eGrantsSearchViewModel
            {
                admincode = string.IsNullOrWhiteSpace(adminCode) || adminCode == "undefined" ? string.Empty : adminCode,
                FilterMechanism = mechanism,
                FilterAdminCode = adminCode
            };

            if (int.TryParse(fy, out int parsedFy))
                viewModel.FilterFY = parsedFy;
            else
                fy = null;

            if (int.TryParse(serialNum, out int parsedSerial))
                viewModel.FilterSerialNumber = parsedSerial;
            else
                serialNum = null;

            viewModel.ICList = await _commonService.LoadAdminCodes();

            var dataList = await _eGrantsService.LoadDataAutocomplete(type, term, mechanism, fy, adminCode, serialNum);

            return Json(dataList);
        }

        // load documents by appl_id
        /// <summary>
        /// The load docs grid.
        /// </summary>
        /// <param name="applId">
        /// The appl_id.
        /// </param>
        /// <param name="searchType">
        /// The search_type.
        /// </param>
        /// <param name="categoryList">
        /// The category_list.
        /// </param>
        /// <param name="mode">
        /// The mode.
        /// </param>
        /// <returns>
        /// The <see cref="JsonResult"/>.
        /// </returns>
        /// 
        public JsonResult LoadDocsGrid(int applId, string searchType = null, string categoryList = null, string mode = null)
        {
            var docs = _documentService.LoadDocs(applId, searchType, categoryList, mode, HttpContext.Session);
            return Json(new { data = docs });
        }

        public JsonResult LoadDocsGridForDownload(int appl_id, string search_type = null, string category_list = null, string mode = null)
        {
            //Search_by_appl_id.LoadDocs(
            //    appl_id,
            //    search_type,
            //    category_list,
            //    Convert.ToString(this.Session["ic"]),
            //    Convert.ToString(this.Session["userid"]));

            //ViewBag.doclayer = Search_by_appl_id.doclayerproperty;

            //// ViewBag.doclayer = Search_by_appl_id.doclayerproperty.ToList();
            //dynamic res = new { data = ViewBag.doclayer };

            //return Json(res, JsonRequestBehavior.AllowGet);
            return Json(null);
        }

        //    /// <summary>
        //    /// The stop_notice.
        //    /// </summary>
        //    /// <param name="grant_id">
        //    /// The grant_id.
        //    /// </param>
        //    /// <returns>
        //    /// The <see cref="ActionResult"/>.
        //    /// </returns>
        //    public ActionResult stop_notice(int grant_id)
        //    {
        //        ViewBag.StopNotice = Dashboard.Functions.Egrants.LoadStopNotice(grant_id, Convert.ToString(this.Session["ic"]));

        //        return View("~/Egrants/Views/_Modal_Stop_Notice.cshtml");
        //    }

        /// <summary>
        /// The supplement.
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<ActionResult> supplement(int grant_id)
        {
            var act = "to_view";
            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext.Session);

            List<supplement> supplements = await _eGrantsService.GetSupplements(act,
                grant_id,
                0,
                string.Empty,
                string.Empty,
                0,
                sessionInfo.Ic,
                sessionInfo.UserId);

            SupplementObjectViewModel supplementObjectViewModel = new SupplementObjectViewModel();

            supplementObjectViewModel.GrantID = grant_id;
            supplementObjectViewModel.Act = act;
            supplementObjectViewModel.Supplement = supplements;
            supplementObjectViewModel.FormerAppls = new List<former_appls>();

            return View("~/Views/eGrants/_Modal_Supplement.cshtml", supplementObjectViewModel);
        }

        //    public string impac_docs_data(string act, int appl_id)
        //    {
        //        try
        //        {
        //            ViewBag.ImpacDocs = EgrantsDoc.LoadImpacDocs(act, appl_id);
        //            ViewBag.act = act;
        //            ViewBag.appl_id = appl_id;

        //            List<ImpacDocs> list = EgrantsDoc.LoadImpacDocs(act, appl_id);
        //            return JsonConvert.SerializeObject(list);
        //        }
        //        catch (Exception err)
        //        {
        //            Console.WriteLine(err);
        //        }

        //        return null;
        //    }

        //    public string doc_attachments_data(int document_id)
        //    {
        //        try
        //        {

        //            List<DocAttachment> list = EgrantsDoc.LoadDocAttachments(document_id);

        //            return JsonConvert.SerializeObject(list);

        //        }
        //        catch (Exception err)
        //        {
        //            Console.WriteLine(err);
        //        }

        //        return null;
        //    }
        //}


        //class MyWebClient : WebClient
        //{
        //    protected override WebRequest GetWebRequest(Uri address)
        //    {
        //        var cert_url = ConfigurationManager.ConnectionStrings["certPath"].ToString();
        //        var cert_pass = ConfigurationManager.ConnectionStrings["certPass"].ToString();
        //        var certificate = new X509Certificate2(cert_url, cert_pass);

        //        HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);

        //        if (request != null)
        //        {
        //            request.ClientCertificates.Add(certificate);
        //        }

        //        return request;
        //    }
    }
}