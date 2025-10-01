#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  EgrantsController.cs
// Solution: eGrants
// Project:  eGrants
// Created: 2022-08-01
// Contributors:
//      - Dehuff, Daryl (NIH/NCI) [C] - dehuffdc
//      -
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

        //private readonly AppDbContext _context;
        private readonly IeGrantsService _eGrantsService;
        private readonly ICommonService _commonService;
        private readonly ISessionInfoService _sessionInfoService;

        public EgrantsController(IeGrantsService eGrantsService, ICommonService commonService, ISessionInfoService sessionInfoService)
        {
            _eGrantsService = eGrantsService;
            _commonService = commonService;
            _sessionInfoService = sessionInfoService;
        }

        //public EgrantsController(AppDbContext context, IeGrantsService eGrantsService, ICommonService commonService)
        //{
        //    _context = context;
        //    _eGrantsService = eGrantsService;
        //    _commonService = commonService;
        //}

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
        /// <param name="fy">
        /// The fy.
        /// </param>
        /// <param name="mechanism">
        /// The mechanism.
        /// </param>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
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
            // string fy, string mechan, s
            var list = await _eGrantsService.GetYearList(fiscalYear, mechanism, adminCode, serialNumber);

            foreach(GrantDataYears val in list) 
            {
                yearList.Add(val.full_grant_num + ":" + val.appl_id);
            }

            // JavaScriptSerializer js = new JavaScriptSerializer();
            return JsonConvert.SerializeObject(yearList);
        }

        //    // load all appls list with or without documents
        //    /// <summary>
        //    /// The get all appls list.
        //    /// </summary>
        //    /// <param name="admin_code">
        //    /// The admin_code.
        //    /// </param>
        //    /// <param name="serial_num">
        //    /// The serial_num.
        //    /// </param>
        //    /// <returns>
        //    /// The <see cref="string"/>.
        //    /// </returns>
        //    public string GetAllApplsList(string admin_code, string serial_num)
        //    {
        //        // string fy, string mechan, s
        //        var list = EgrantsAppl.GetAllApplsList(admin_code, serial_num);

        //        // JavaScriptSerializer js = new JavaScriptSerializer();
        //        return JsonConvert.SerializeObject(list);
        //    }

        //    // get category list by grant_id and years
        //    /// <summary>
        //    /// The load categories.
        //    /// </summary>
        //    /// <param name="grant_id">
        //    /// The grant_id.
        //    /// </param>
        //    /// <param name="years">
        //    /// The years.
        //    /// </param>
        //    /// <returns>
        //    /// The <see cref="string"/>.
        //    /// </returns>
        //    public string LoadCategories(int grant_id, string years)
        //    {
        //        var list = Dashboard.Functions.Egrants.GetCategoryList(grant_id, years);

        //        // JavaScriptSerializer js = new JavaScriptSerializer();
        //        return JsonConvert.SerializeObject(list);
        //    }

        //    // get category list by grant_id and years
        //    /// <summary>
        //    /// The load categories.
        //    /// </summary>
        //    /// <param name="name">
        //    /// The new label for the grant year
        //    /// </param>
        //    /// <param name="applId">
        //    /// The appl_id for the grant year about to be renamed
        //    /// </param>
        //    /// <returns>
        //    /// The function returns true if successful<see cref="bool"/>.
        //    /// </returns>
        //    public bool NewGrantYearName(string name, int applId)
        //    {
        //        if (string.IsNullOrEmpty(name))
        //        {
        //            name = string.Empty;
        //        }
        //        var length = name.Length;
        //        var truncatedName = name.Substring(0, Math.Min(length,10));

        //        Dashboard.Functions.Egrants.SetGrantYearLabel(name, applId);

        //        return true;
        //    }

        //    //public CountProperty<int> CountProperty;// = new CountProperty<int>();
        //    //countProperty.Value = 0;

        /// <summary>
        /// The by_str.
        /// </summary>
        /// <param name="aStr">
        /// The str.
        /// </param>
        /// <param name="aMode">
        /// The mode.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> by_str(string aStr, string aMode = null)
        {
            //TODO: Determine if the following code is ever being used
            // CountProperty = new CountProperty<int>();
            // CountProperty.Value = 0;

            eGrantsSearchViewModel eGrantsSearchViewModelList = new eGrantsSearchViewModel();

            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext);

            //try
            //{
                if (!HttpContext.Session.TryGetValue("ic", out var icbytes)) sessionInfo.Ic = "";
                if (!HttpContext.Session.TryGetValue("browser", out var browserbytes)) sessionInfo.Browser = "";
                if (!HttpContext.Session.TryGetValue("userid", out var useridbytes)) sessionInfo.UserId = "";

                eGrantsSearchViewModelList = await _eGrantsService.GetEgrantsByStrAsync(aStr, 0, 0, 0, sessionInfo.Browser, sessionInfo.Ic, sessionInfo.UserId);

                if (eGrantsSearchViewModelList.grantlayerproperty != null)
                {
                    // show pagination
                    eGrantsSearchViewModelList.Pagination = await _eGrantsService.LoadPagination(
                            aStr,
                            sessionInfo.Ic, 
                            sessionInfo.UserId,
                            string.Empty);
                }
                else
                {
                    eGrantsSearchViewModelList.Message = "No data found for the search";
                    eGrantsSearchViewModelList.grantlayer = null;
                }

                eGrantsSearchViewModelList.Mode = aMode;
                eGrantsSearchViewModelList.ICList = await _commonService.LoadAdminCodes();
                return View("~/Views/Index.cshtml", eGrantsSearchViewModelList);
            //}
            //catch (Exception ex)
            //{
            //    ////return View("Error");
            //    //// Option 1: Use ViewData
            //    ////ViewData["ErrorMessage"] = "Something went wrong while processing your request.";
            //    eGrantsSearchViewModelList.ICList = await _commonService.LoadAdminCodes();

            //    //// Option 2: Use ModelState
            //    //ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");

            //    //// Return the same view with the existing model (if partially populated)
            //    //return View("~/Views/Index.cshtml", eGrantsSearchViewModelList);

            //    TempData["ErrorMessage"] = "Oops! Something went wrong while processing your request.";
            //    return View("~/Views/Index.cshtml", eGrantsSearchViewModelList);
            //}
        }

        /// <summary>
        /// The by_grant.
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <param name="package">
        /// The package.
        /// </param>
        /// <param name="categories">
        /// The categories.
        /// </param>
        /// <param name="appls_list">
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
            int aGrantId = 0,
            string aPackage = null,
            string aCategories = null,
            string aApplsList = null,
            string aYears = null,
            string aMode = null)
        {
            eGrantsSearchViewModel eGrantsSearchViewModelList = new eGrantsSearchViewModel();

            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext);

            //ViewBag.ICList = EgrantsCommon.LoadAdminCodes();
            var isExisting = await _eGrantsService.CheckGrantID(aGrantId);

            if (!HttpContext.Session.TryGetValue("ic", out var icbytes)) sessionInfo.Ic = "";
            if (!HttpContext.Session.TryGetValue("browser", out var browserbytes)) sessionInfo.Browser = "";
            if (!HttpContext.Session.TryGetValue("userid", out var useridbytes)) sessionInfo.UserId = "";

            //return View("~/Views/Index.cshtml", eGrantsSearchViewModelList);

            if (aGrantId == 0 || isExisting == 0)
            {
                eGrantsSearchViewModelList.Message = "No data found for the search";
                eGrantsSearchViewModelList.grantlayer = null;
            }
            else
            {
                // load data from DB
                eGrantsSearchViewModelList = await _eGrantsService.GetEgrantsByGrantAsync(string.Empty, aGrantId, aPackage, 0, 0, sessionInfo.Browser, sessionInfo.Ic, sessionInfo.UserId);

                eGrantsSearchViewModelList.bygrant = 1;
                eGrantsSearchViewModelList.GrantID = aGrantId;
                eGrantsSearchViewModelList.Package = aPackage;
                eGrantsSearchViewModelList.Mode = aMode;
                eGrantsSearchViewModelList.SearchStyle = "by_grant";
                eGrantsSearchViewModelList.SelectedYears = aYears;
                eGrantsSearchViewModelList.SelectedCats = aCategories;

                if (aCategories == string.Empty || aCategories == "All" || aCategories == "all")
                    eGrantsSearchViewModelList.SelectedCategories = "All";
                else if (aCategories != string.Empty && aCategories != "All" && aCategories != "all")
                    eGrantsSearchViewModelList.SelectedCategories = await _eGrantsService.GetCategoryNameById(aCategories);

                eGrantsSearchViewModelList.grantlayer = eGrantsSearchViewModelList.grantlayerproperty;
                eGrantsSearchViewModelList.appllayer_All = eGrantsSearchViewModelList.appllayerproperty;
                eGrantsSearchViewModelList.appllayer = eGrantsSearchViewModelList.appllayerproperty;
                eGrantsSearchViewModelList.ApplCount = eGrantsSearchViewModelList.appllayer.Count;
                eGrantsSearchViewModelList.doclayer = eGrantsSearchViewModelList.doclayerproperty;
                eGrantsSearchViewModelList.DocCount = eGrantsSearchViewModelList.doclayer.Count;

                // set appls_lis for searching by flag_type
                if (aPackage != string.Empty && aPackage != "All" && aPackage != "all")
                {
                    var filterSearchResult = await _eGrantsService.GetApplsList(aGrantId, aPackage);
                    aApplsList = filterSearchResult.Select(x => x.Value).FirstOrDefault();
                }

                // set appls_lis for searching by years
                if (aYears != string.Empty)
                {
                    if (aYears == "all" || aYears == "All")
                        aApplsList = "All";
                    else
                    {
                        var filterSearchResult = await _eGrantsService.GetApplsList(aGrantId, null, aYears);
                        aApplsList = filterSearchResult.Select(x => x.Value).FirstOrDefault();
                    }
                    //  aApplsList = EgrantsAppl.GetApplsList(grant_id, null, aYears);
                }

                eGrantsSearchViewModelList.SelectedAppls = aApplsList;

                // reset appllayer and limit show appls if appls_list with search parameters
                if (aApplsList != null && !aApplsList.Equals("All", StringComparison.InvariantCultureIgnoreCase))
                {
                    var appllist = new List<ApplLayerObject>();

                    // for more than one appl
                    if (aApplsList.IndexOf(',') > 1)
                    {
                        var app = aApplsList.Split(',').ToList();

                        // List<Egrants.Models.Egrants.appllayer> appllist = new List<Egrants.Models.Egrants.appllayer>();
                        foreach (var appl in eGrantsSearchViewModelList.appllayer)
                        {
                            if (app.Any(n => n == appl.appl_id))
                            {
                                appl.display_docs = "y";
                                appllist.Add(appl);
                            }
                        }

                        eGrantsSearchViewModelList.appllayer = appllist;
                    }

                    // for only one appl
                    else
                    {
                        // ViewBag.ApplID = appls_list;
                        var app = aApplsList.Split().ToList();

                        // List<Egrants.Models.Egrants.appllayer> appllist = new List<Egrants.Models.Egrants.appllayer>();
                        foreach (var appl in eGrantsSearchViewModelList.appllayer)
                            if (app.Any(n => n == appl.appl_id))
                            {
                                appl.display_docs = "y";
                                appllist.Add(appl);
                            }
                        eGrantsSearchViewModelList.appllayer = appllist;
                    }
                }
                else if (aApplsList != null && aApplsList.Equals("All", StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (var appl in eGrantsSearchViewModelList.appllayer)
                    {
                        appl.display_docs = "y";
                    }
                }
            }

            eGrantsSearchViewModelList.Mode = aMode;
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
        /// <param name="aFiscalYear">
        /// The fiscalYear.
        /// </param>
        /// <param name="aMechanism">
        /// The mechanism.
        /// </param>
        /// <param name="aAdminCode">
        /// The adminCode.
        /// </param>
        /// <param name="aSerialNum">
        /// The serialNumber.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public async Task<IActionResult> by_filters(int aFiscalYear = 0, string aMechanism = null, string aAdminCode = null, int aSerialNum = 0)
        {
            eGrantsSearchViewModel eGrantsSearchViewModelList = new eGrantsSearchViewModel();

            var sessionInfo = _sessionInfoService.GetSessionInfo(HttpContext);

            eGrantsSearchViewModelList = await _eGrantsService.GetEgrantsByFilterAsync(aFiscalYear, aMechanism, aSerialNum, aAdminCode, 0, 0, 0, sessionInfo.Browser, sessionInfo.Ic, sessionInfo.UserId);
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

        //    /// <summary>
        //    /// The by_page.
        //    /// </summary>
        //    /// <param name="str">
        //    /// The str.
        //    /// </param>
        //    /// <param name="tab_num">
        //    /// The tab_num.
        //    /// </param>
        //    /// <param name="page_num">
        //    /// The page_num.
        //    /// </param>
        //    /// <param name="package">
        //    /// The package.
        //    /// </param>
        //    /// <param name="mode">
        //    /// The mode.
        //    /// </param>
        //    /// <returns>
        //    /// The <see cref="ActionResult"/>.
        //    /// </returns>
        //    public ActionResult by_page(string str = null, int tab_num = 0, int page_num = 0, string package = null, string mode = null)
        //    {
        //        ViewBag.ICList = EgrantsCommon.LoadAdminCodes();

        //        if (string.IsNullOrEmpty(str))
        //        {
        //            ViewBag.Message = "No data found for the search";
        //            ViewBag.grantlayer = null;
        //        }
        //        else if (page_num == 0 || tab_num == 0)
        //        {
        //            ViewBag.Message = "No data found for the search";
        //            ViewBag.grantlayer = null;
        //        }
        //        else
        //        {
        //            ViewBag.SearchStyle = "by_page";
        //            ViewBag.CurrentTab = tab_num;
        //            ViewBag.CurrentPage = page_num;
        //            ViewBag.Str = str;
        //            ViewBag.Mode = mode;

        //            Search.egrants_search(
        //                str,
        //                0,
        //                string.Empty,
        //                0,
        //                page_num,
        //                Convert.ToString(this.Session["browser"]),
        //                Convert.ToString(this.Session["ic"]),
        //                Convert.ToString(this.Session["userid"]));

        //            ViewBag.grantlayer = Search.grantlayerproperty;
        //            ViewBag.appllayer = Search.appllayerproperty;
        //            ViewBag.appllayer_All = Search.appllayerproperty;
        //            ViewBag.ApplCount = ViewBag.appllayer.Count;
        //            ViewBag.doclayer = Search.doclayerproperty;
        //            ViewBag.DocCount = ViewBag.doclayer.Count;

        //            if (str == "qc")
        //                ViewBag.Mode = "qc";

        //            // show Pagination 
        //            ViewBag.Pagination = Dashboard.Functions.Egrants.LoadPagination(
        //                str,
        //                Convert.ToString(this.Session["ic"]),
        //                Convert.ToString(this.Session["userid"]),
        //                package);

        //            if (str == "qc")
        //                ViewBag.UnidentifiedDocs = EgrantsDoc.LoadDocsUnidentified(
        //                    Convert.ToString(this.Session["ImageServerUrl"]),
        //                    Convert.ToString(this.Session["userid"]));
        //        }

        //        return View("~/Egrants/Views/Index.cshtml");
        //    }

        //    // Autocomplete for fy, activity_code and serial_number
        //    /// <summary>
        //    /// The load_data_autocomplete.
        //    /// </summary>
        //    /// <param name="type">
        //    /// The type.
        //    /// </param>
        //    /// <param name="term">
        //    /// The term.
        //    /// </param>
        //    /// <param name="mechanism">
        //    /// The mechanism.
        //    /// </param>
        //    /// <param name="fy">
        //    /// The fy.
        //    /// </param>
        //    /// <param name="admincode">
        //    /// The admincode.
        //    /// </param>
        //    /// <param name="serialnum">
        //    /// The serialnum.
        //    /// </param>
        //    /// <returns>
        //    /// The <see cref="JsonResult"/>.
        //    /// </returns>
        //    public JsonResult load_data_autocomplete(
        //        string type,
        //        string term,
        //        string mechanism = null,
        //        string fy = null,
        //        string admincode = null,
        //        string serialnum = null)
        //    {
        //        var sql_query = string.Empty;

        //        // List<string> data_list = new List<string>();
        //        if (admincode != null && admincode != string.Empty)
        //            ViewBag.admincode = admincode;

        //        if (admincode == "undefined")
        //            ViewBag.admincode = string.Empty;
        //        else
        //            ViewBag.admincode = string.Empty;

        //        ViewBag.FilterFY = fy;

        //        ViewBag.FilterSerialNumber = serialnum;

        //        ViewBag.FilterMechanism = mechanism;

        //        ViewBag.FilterAdminCode = admincode;
        //        ViewBag.ICList = EgrantsCommon.LoadAdminCodes();
        //        var data_list = new List<string>();

        //        using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
        //        {
        //            // if (type == "fy")
        //            // {
        //            // sql_query = "sp_web_egrants_load_data_autocomplete";
        //            // }
        //            if (type == "mechanism")
        //                sql_query = "sp_web_egrants_load_data_autocomplete_mechanism";

        //            if (type == "serialnum")
        //                sql_query = "sp_web_egrants_load_data_autocomplete_serialnum";

        //            if (type == "fy")
        //                sql_query = "sp_web_egrants_load_data_autocomplete_fy";

        //            var cmd = new SqlCommand(sql_query, conn);
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Parameters.AddWithValue("@term", term);
        //            cmd.Parameters.AddWithValue("@fy", fy);
        //            cmd.Parameters.AddWithValue("@mechanism", mechanism);
        //            cmd.Parameters.AddWithValue("@admincode", admincode);
        //            cmd.Parameters.AddWithValue("@serialnum", serialnum);
        //            conn.Open();
        //            var rdr = cmd.ExecuteReader();

        //            while (rdr.Read())
        //                data_list.Add(rdr[0].ToString());

        //            // sql_query = rdr[0].ToString();
        //        }

        //        return this.Json(data_list, JsonRequestBehavior.AllowGet);
        //    }

        // load documents by appl_id
        /// <summary>
        /// The load docs grid.
        /// </summary>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <param name="search_type">
        /// The search_type.
        /// </param>
        /// <param name="category_list">
        /// The category_list.
        /// </param>
        /// <param name="mode">
        /// The mode.
        /// </param>
        /// <returns>
        /// The <see cref="JsonResult"/>.
        /// </returns>
        public JsonResult LoadDocsGrid(int appl_id, string search_type = null, string category_list = null, string mode = null)
        {
            Exception exceptionKeeper = null;
            bool completed = false;
            //for (int i = 0; i < MAX_RETRIES; ++i)
            //{
            //    try
            //    {
            //        Search_by_appl_id.LoadDocs(
            //        appl_id,
            //        search_type,
            //        category_list,
            //        Convert.ToString(this.Session["ic"]),
            //        Convert.ToString(this.Session["userid"]));
            //        completed = true;
            //        break;
            //    }
            //    catch (Exception ex)
            //    {
            //        exceptionKeeper = ex;
            //        // 5 retries, ok now log and deal with the error.
            //    }
            //}
            //if (!completed)
            //    throw exceptionKeeper;

            //ViewBag.doclayer = Search_by_appl_id.doclayerproperty;

            //// ViewBag.doclayer = Search_by_appl_id.doclayerproperty.ToList();
            //dynamic res = new { data = ViewBag.doclayer };

            //return Json(res, JsonRequestBehavior.AllowGet);
            return Json(null);
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

        //    /// <summary>
        //    /// The supplement.
        //    /// </summary>
        //    /// <param name="grant_id">
        //    /// The grant_id.
        //    /// </param>
        //    /// <returns>
        //    /// The <see cref="ActionResult"/>.
        //    /// </returns>
        //    public ActionResult supplement(int grant_id)
        //    {
        //        var act = "to_view";

        //        ViewBag.StopNotice = Dashboard.Functions.Egrants.LoadSupplement(
        //            act,
        //            grant_id,
        //            0,
        //            string.Empty,
        //            string.Empty,
        //            0,
        //            Convert.ToString(this.Session["ic"]),
        //            Convert.ToString(this.Session["userid"]));

        //        return View("~/Egrants/Views/_Modal_Supplement.cshtml");
        //    }

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