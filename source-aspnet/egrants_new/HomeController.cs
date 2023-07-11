#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  EgrantsController.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-05-05
// Contributors:
//      - Briggs, Robin (NIH/NCI) [C] - briggsr2
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

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web.Mvc;

using egrants_new.Egrants.Models;
using egrants_new.Functions;
using egrants_new.Models;

using Newtonsoft.Json;
using Rotativa;

#endregion

namespace egrants_new.Controllers
{
    /// <summary>
    /// The egrants controller.
    /// </summary>
    public class HomeController : Controller
    {


        // go to default 
        /// <summary>
        /// The go_to_default.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.CIS
        /// </returns>
        public ActionResult Go_to_default()
        {
            return this.View("~/Views/Shared/Go_to_Default.cshtml");
        }

        // GET: Egrants
        /// <summary>   
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            // return IC list
            this.ViewBag.ICList = EgrantsCommon.LoadAdminCodes();
            this.ViewBag.CurrentView = "StandardForm";

            return this.View("~/Views/Egrants/Index.cshtml");
        }

        public string SetCurrentViewSessionVariable(string currentView)
        {
            Console.WriteLine("In setting session Variable: " + currentView);
            Session["CurrentView"] = currentView;

            return currentView;
        }

        /// <summary>
        /// HttpPost
        /// Download the files to the temp directory from the links checked on the page. Then return the stream of bytes to the calling method.
        /// </summary>
        /// <param name="appl"></param>
        /// <param name="listOfUrl"></param>
        /// <returns></returns>
        public ActionResult IsDownloadForm(string appl, string fullGrantNumber, IList<string> listOfUrl)
        {
            // 1 - trim the first character in the full grant number
            // 2 - trim the characters in full grant number year, and anything after trim
            string downloadDirectory;

            // The downloadModel contains all of the data that will be returned to the view
            DownloadModel downloadModel = new DownloadModel();

            try
            {

                downloadModel.ApplId = appl;
                downloadModel.NumFailed = 0;
                downloadModel.NumSucceeded = 0;
                downloadModel.NumToDownload = listOfUrl.Count();

                // create the temp path and
                downloadDirectory = Path.Combine(Path.GetTempPath(), appl);

                // create or return an existing directory to hold the downloaded files
                DirectoryInfo directoryInfo = Directory.CreateDirectory(downloadDirectory);

                // delete all the files in this directory if there are any
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }

                // delete all the folders in this directory if there are any
                foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            catch (ArgumentNullException ex)
            {
                downloadModel.Error = "There are no URLs in the list!";

                return Json(downloadModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                downloadModel.Error
                    = "General Exception. This is likely an error in accessing temp files and temp directories! Notify Development Team of this error.";

                return Json(downloadModel, JsonRequestBehavior.AllowGet);
            }

            // var grantId = this.ViewBag.GrantID;
            DownloadData downloadData = new DownloadData();
            downloadModel.DownloadDataList = new List<DownloadData>();

            // obtain the document url from the remote system
            var cerUri = ConfigurationManager.ConnectionStrings["certPath"].ToString();
            var certPass = ConfigurationManager.ConnectionStrings["certPass"].ToString();
            var certificate = new X509Certificate2(cerUri, certPass);

            foreach (var dataInput in listOfUrl)
            {
                try
                {
                    downloadData = new DownloadData();

                    var split = dataInput.Split(new char[] { '|' }, StringSplitOptions.None);

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
                    downloadData.DocumentDate = DateTime.TryParse(documentDate, out DateTime result) ? result : DateTime.MinValue;



                    // if(downloadModel.DownloadDataList.)
                    // get a temp file to save the downloaded file
                    string tmpFileName = Path.GetTempFileName();

                    // if this is an i2e file
                    if (url.Contains("https://i2e"))
                    {

                        Console.WriteLine("We should never hit this....");

                        throw new Exception("We found an i2e path and these should not be included in downloads");
                    }

                    // if this is a file on the ERA Server
                    if (url.Contains("https://s2s."))
                    {
                        var uri = new Uri(url);

                        // obtain the document url from the remote system
                        // var cerUri = ConfigurationManager.ConnectionStrings["certPath"].ToString();
                        // var certPass = ConfigurationManager.ConnectionStrings["certPass"].ToString();
                        // var certificate = new X509Certificate2(cerUri, certPass);

                        var webRequest = (HttpWebRequest)WebRequest.Create(uri);
                        webRequest.KeepAlive = false;
                        webRequest.Method = "GET";
                        webRequest.AllowAutoRedirect = false;
                        webRequest.ClientCertificates.Add(certificate);

                        var webResponse = (HttpWebResponse)webRequest.GetResponse();

                        using (var postStream = webResponse.GetResponseStream())
                        {
                            if (postStream == null)
                            {
                                throw new Exception("The stream was empty!");
                            }

                            string downloadUrl;

                            using (var reader = new StreamReader(postStream))
                            {
                                downloadUrl = reader.ReadToEnd();
                            }

                            using (var myWebClient = new MyWebClient())
                            {
                                myWebClient.Credentials = CredentialCache.DefaultCredentials;

                                // Download the Web resource and save it into the current filesystem folder.
                                myWebClient.DownloadFile(downloadUrl, tmpFileName);

                                // // get the filename from the content-disposition header of the downloaded file
                                var disposition = myWebClient.ResponseHeaders["Content-Disposition"];
                                ContentDisposition contentDisposition = new ContentDisposition(disposition);
                                string filename = contentDisposition.FileName;
                                FileInfo fi = new FileInfo(filename);

                                string newFileName = string.Empty;

                                if (category == "Financial Report")
                                {
                                    newFileName = ReplaceInvalidChars(
                                        $"{fullGrantNumber.Remove(0, 4)}-{documentName}-{Convert.ToDateTime(documentDate):MM-dd-yyyy}-{Path.GetFileNameWithoutExtension(fi.Name)}{fi.Extension}",
                                        "_");
                                }
                                else
                                {
                                    // just reove the first four characters which are the first digit, the P30 part, concat the document_name and the file extention
                                    // and remove all invalid characters from filename and replace with _
                                    newFileName = ReplaceInvalidChars(
                                        $"{fullGrantNumber.Remove(0, 4)}-{documentName}-{documentId}{fi.Extension}",
                                        "_");
                                }

                                // move the file from the temp file to a file with the filename in the downloadDirectory
                                System.IO.File.Move(tmpFileName, Path.Combine(downloadDirectory, newFileName));
                                downloadData.FileDownloaded = newFileName;
                            }
                        }

                        downloadModel.NumSucceeded += 1;
                    }
                    else
                    {

                        Uri uri;

                        if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                        {
                            var imageServer = new Uri(this.Session["ImageServerUrl"].ToString());

                            uri = new Uri(imageServer, url);
                        }

                        if (category == "CloseoutNotification" || category == "FFR_REJECTION")
                        {
                            this.ViewBag.notification = EgrantsDoc.getCloseoutNotif(appl, documentName);
                            this.ViewBag.applid = appl;

                            var report = new ViewAsPdf("~/Egrants/Views/CloseoutNotif.cshtml");
                            byte[] bytes = report.BuildFile(this.ControllerContext);


                            string newFileName = string.Empty;

                            // just remove the first four characters which are the first digit, the P30 part, concat the document_name and the file extension
                            // and remove all invalid characters from filename and replace with _
                            if (category == "CloseoutNotification")
                            {
                                newFileName = ReplaceInvalidChars(
                                    $"{fullGrantNumber.Remove(0, 4)}-{category}-{documentName}-{Convert.ToDateTime(documentDate):MM-dd-yyyy}.pdf",
                                    "_");
                            }

                            if (category == "FFR_REJECTION")
                            {
                                newFileName = ReplaceInvalidChars(
                                    $"{fullGrantNumber.Remove(0, 4)}-{documentName}-{Convert.ToDateTime(documentDate):MM-dd-yyyy}.pdf",
                                    "_");
                            }

                            System.IO.File.WriteAllBytes(tmpFileName, bytes);



                            // move the file from the temp file to a file with the filename in the downloadDirectory
                            System.IO.File.Move(tmpFileName, Path.Combine(downloadDirectory, newFileName));
                            downloadData.FileDownloaded = newFileName;
                        }
                        else
                        {
                            using (var myWebClient = new WebClient())
                            {
                                myWebClient.UseDefaultCredentials = true;
                                myWebClient.Credentials = CredentialCache.DefaultNetworkCredentials;
                                myWebClient.Credentials = CredentialCache.DefaultCredentials;

                                myWebClient.Headers.Add(HttpRequestHeader.Cookie, Request.Headers["cookie"]);

                                myWebClient.DownloadFile(uri, tmpFileName);
                                string filename = Path.GetFileName(uri.LocalPath);
                                FileInfo fi = new FileInfo(filename);

                                string newFileName = string.Empty;

                                // just remove the first four characters which are the first digit, the P30 part, concat the document_name and the file extension
                                // and remove all invalid characters from filename and replace with _
                                newFileName = ReplaceInvalidChars($"{fullGrantNumber.Remove(0, 4)}-{documentName}-{documentId}{fi.Extension}", "_");

                                // move the file from the temp file to a file with the filename in the downloadDirectory
                                System.IO.File.Move(tmpFileName, Path.Combine(downloadDirectory, newFileName));
                                downloadData.FileDownloaded = newFileName;
                            }
                        }

                        downloadModel.NumSucceeded += 1;
                    }
                }
                catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
                {
                    // code specifically for a WebException NotFound
                    downloadData.Error = "File not found.";
                }
                catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.InternalServerError)
                {
                    // code specifically for a WebException InternalServerError
                    downloadData.Error = "Internal Server Error! Notify Dev Team!";
                }
                catch (ArgumentNullException ex)
                {
                    downloadModel.Error = "An value is null which should not be.";
                }
                catch (Exception err)
                {
                    downloadData.Error = "General Exception! Screenshot and this message and notify the Development Team: " + Environment.NewLine
                                       + err.Message.ToString();

                    downloadModel.NumFailed += 1;
                }

                downloadModel.DownloadDataList.Add(downloadData);
            }

            string handle = Guid.NewGuid().ToString();
            downloadModel.Handle = handle;

            string zipFileName = fullGrantNumber.Remove(0, 1) + ".zip";
            string zipFileNameWithPath = Path.Combine(Path.GetTempPath(), zipFileName);

            downloadModel.ZipFilename = zipFileName;

            try
            {
                // if the zip file exists delete it
                if (System.IO.File.Exists(zipFileNameWithPath))
                {
                    System.IO.File.Delete(zipFileNameWithPath);
                }

                // zip the contents of the downloadDirectory to the zipPath
                ZipFile.CreateFromDirectory(downloadDirectory, zipFileNameWithPath);

                using (MemoryStream ms = new MemoryStream())
                using (FileStream file = new FileStream(zipFileNameWithPath, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[file.Length];
                    file.Read(bytes, 0, (int)file.Length);
                    ms.Write(bytes, 0, (int)file.Length);
                    TempData[handle] = ms.ToArray();
                }
            }
            catch (Exception err)
            {
                Console.WriteLine("Error trying to Zip or serve zip file: " + err.ToString());
                downloadModel.Error = "ZIP FILE ERROR! Screen shot this error and send to Dev team! " + Environment.NewLine + err.ToString();
            }

            return Json(downloadModel, JsonRequestBehavior.AllowGet);
        }

        /// <summary>  
        /// Override the JSON Result with Max integer JSON lenght  
        /// </summary>  
        /// <param name="data">Data</param>  
        /// <param name="contentType">Content Type</param>  
        /// <param name="contentEncoding">Content Encoding</param>  
        /// <param name="behavior">Behavior</param>  
        /// <returns>As JsonResult</returns>  
        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
                   {
                       Data = data,
                       ContentType = contentType,
                       ContentEncoding = contentEncoding,
                       JsonRequestBehavior = behavior,
                       MaxJsonLength = int.MaxValue
                   };
        }

        // [HttpGet]
        public virtual ActionResult Download(string fileGuid, string fileName)
        {

            if (TempData[fileGuid] != null)
            {
                byte[] data = TempData[fileGuid] as byte[];

                var cd = new ContentDisposition
                         {
                             // for example foo.bak
                             FileName = fileName,

                             // always prompt the user for downloading, set to true if you want 
                             // the browser to try to show the file inline
                             Inline = false,
                         };

                Response.AppendHeader("Content-Disposition", cd.ToString());

                return File(data, "application/zip");
            }
            else
            {
                // Problem - Log the error, generate a blank file,
                //           redirect to another controller action - whatever fits with your application
                return new EmptyResult();
            }
        }

        public string ReplaceInvalidChars(string filename, string replacementCharacter)
        {
            return string.Join(replacementCharacter, filename.Split(Path.GetInvalidFileNameChars()));
        }

        /// <summary>
        /// Get all appls list for appls toggle by grant_id
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string LoadAllAppls(int grant_id)
        {
            List<string> list = EgrantsAppl.GetAllAppls(grant_id);

            // JavaScriptSerializer js = new JavaScriptSerializer();
            return JsonConvert.SerializeObject(list);
        }

        /// <summary>
        /// Load 12 appls list for appls toggle by grant_id
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string LoadDefaultAppls(int grant_id)
        {
            var list = EgrantsAppl.GetDefaultAppls(grant_id);

            // JavaScriptSerializer js = new JavaScriptSerializer();
            return JsonConvert.SerializeObject(list);
        }

        // get appls list with documents by (admin_code and serial_num) commented out by Leon at 3/15/2019
        // public string LoadYears(string admin_code, string serial_num)   //string fy, string mechan, s
        // {
        // List<string> yearlist = Egrants.Models.Egrants.GetYearList(admin_code, serial_num);
        // JavaScriptSerializer js = new JavaScriptSerializer();
        // return js.Serialize(yearlist);           
        // }

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
        public string LoadYears(string fiscalYear = null, string mechanism = null, string adminCode = null, string serialNumber = null)
        {
            // string fy, string mechan, s
            var list = Dashboard.Functions.Egrants.GetYearList(fiscalYear, mechanism, adminCode, serialNumber);

            // JavaScriptSerializer js = new JavaScriptSerializer();
            return JsonConvert.SerializeObject(list);
        }

        // load all appls list with or without documents
        /// <summary>
        /// The get all appls list.
        /// </summary>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetAllApplsList(string admin_code, string serial_num)
        {
            // string fy, string mechan, s
            var list = EgrantsAppl.GetAllApplsList(admin_code, serial_num);

            // JavaScriptSerializer js = new JavaScriptSerializer();
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
        public string LoadCategories(int grant_id, string years)
        {
            var list = Dashboard.Functions.Egrants.GetCategoryList(grant_id, years);

            // JavaScriptSerializer js = new JavaScriptSerializer();
            return JsonConvert.SerializeObject(list);
        }
    }
}