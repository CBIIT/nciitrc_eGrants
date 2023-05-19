#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  InstitutionalFilesController.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-03-31
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
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;

using egrants_new.Egrants.Models;

#endregion

namespace egrants_new.Controllers
{
    /// <summary>
    /// The institutional files controller.
    /// </summary>
    public class InstitutionalFilesController : Controller
    {
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public ActionResult Index()
        {
            var repository = new InstitutionalFilesRepo();

            // Create new Page Model to adhere to MVC practices
            // Should have a Builder but... This will do for now
            var page = new InstitutionalFilesPage
                           {
                               SelectedInstitutionalOrg = new InstitutionalOrg(),
                               Action = InstitutionalFilesPageAction.ShowOrgs,
                               CharacterIndices = repository.LoadOrgNameCharacterIndices(),
                               OrgList = repository.LoadOrgList(2)
                           };

            return this.View("~/Egrants/Views/InstitutionalFilesIndex.cshtml", page);
        }

        /// <summary>
        /// The show_ orgs.
        /// </summary>
        /// <param name="index_id">
        /// The index_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public ActionResult Show_Orgs(int index_id)
        {
            var repository = new InstitutionalFilesRepo();

            var page = new InstitutionalFilesPage
                           {
                               SelectedInstitutionalOrg = new InstitutionalOrg(),
                               Action = InstitutionalFilesPageAction.ShowOrgs,
                               CharacterIndices = repository.LoadOrgNameCharacterIndices(),
                               OrgList = repository.LoadOrgList(index_id)
                           };

            return this.View("~/Egrants/Views/InstitutionalFilesIndex.cshtml", page);
        }

        /// <summary>
        /// The search_ orgs.
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public ActionResult Search_Orgs(string str)
        {
            var repository = new InstitutionalFilesRepo();

            var page = new InstitutionalFilesPage
                           {
                               SelectedInstitutionalOrg = new InstitutionalOrg(),
                               Action = InstitutionalFilesPageAction.ShowOrgs,
                               CharacterIndices = repository.LoadOrgNameCharacterIndices(),
                               OrgList = repository.SearchOrgList(str)
                           };

            return this.View("~/Egrants/Views/InstitutionalFilesIndex.cshtml", page);
        }

        /// <summary>
        /// The show_ docs.
        /// </summary>
        /// <param name="org_id">
        /// The org_id.
        /// </param>
        /// <param name="org_name">
        /// The org_name.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public ActionResult Show_Docs(int org_id = 0, string org_name = "")
        {
            var repository = new InstitutionalFilesRepo();

            var selectedInstitutionalOrg = repository.FindOrg(org_id, org_name);

            var page = new InstitutionalFilesPage
                           {
                               SelectedInstitutionalOrg = selectedInstitutionalOrg,
                               Action = InstitutionalFilesPageAction.ShowDocs,
                               CharacterIndices = repository.LoadOrgNameCharacterIndices(),
                               DocFiles = repository.LoadOrgDocList(selectedInstitutionalOrg.OrgId)
                           };

            return this.View("~/Egrants/Views/InstitutionalFilesIndex.cshtml", page);
        }

        /// <summary>
        /// The delete_ doc.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="doc_id">
        /// The doc_id.
        /// </param>
        /// <param name="org_id">
        /// The org_id.
        /// </param>
        /// <param name="org_name">
        /// The org_name.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public ActionResult Delete_Doc(string act, int doc_id, int org_id, string org_name)
        {
            var repository = new InstitutionalFilesRepo();

            // disable_doc
            repository.DisableDoc(doc_id, Convert.ToString(this.Session["userid"]));

            this.ViewBag.Act = act;
            this.ViewBag.OrgID = org_id;
            this.ViewBag.OrgName = org_name;

            return this.Show_Docs(org_id, org_name);
        }

        /// <summary>
        /// The show_ create_ doc.
        /// </summary>
        /// <param name="org_id">
        /// The org_id.
        /// </param>
        /// <param name="org_name">
        /// The org_name.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public ActionResult Show_Create_Doc(int org_id, string org_name)
        {
            var repository = new InstitutionalFilesRepo();

            // set act
            var selectedInstitutionalOrg = repository.FindOrg(org_id);

            var page = new InstitutionalFilesPage
                           {
                               SelectedInstitutionalOrg = selectedInstitutionalOrg,
                               Action = InstitutionalFilesPageAction.CreateNew,
                               CharacterIndices = repository.LoadOrgNameCharacterIndices(),
                               DocFiles = repository.LoadOrgDocList(org_id),
                               OrgCategories = repository.LoadOrgCategory(true),
                               TodayText = DateTime.Now.ToShortDateString()
                           };

            return this.View("~/Egrants/Views/InstitutionalFilesIndex.cshtml", page);
        }

        /// <summary>
        /// The show_ update_ doc.
        /// </summary>
        /// <param name="doc_id">
        /// The doc_id.
        /// </param>
        /// <param name="org_id">
        /// The org_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public ActionResult Show_Update_Doc(int doc_id, int org_id)
        {
            var repository = new InstitutionalFilesRepo();
            var selectedInstitutionalOrg = repository.FindOrg(org_id);

            var page = new InstitutionalFilesPage
                           {
                               SelectedInstitutionalOrg = selectedInstitutionalOrg,
                               Action = InstitutionalFilesPageAction.UpdateDoc,
                               CharacterIndices = repository.LoadOrgNameCharacterIndices(),
                               SelectedDocFile = repository.LoadOrgDocList(org_id).Where(d => d.DocumentId == doc_id).FirstOrDefault(),

                               // DocFiles = _repo.LoadOrgDocList(org_id).Where(d => d.DocumentId == doc_id).ToList(),
                               OrgCategories = repository.LoadOrgCategory(false),
                               TodayText = DateTime.Now.ToShortDateString()
                           };

            return this.View("~/Egrants/Views/InstitutionalFilesIndex.cshtml", page);
        }

        /// <summary>
        /// The create_ doc_by_ d drop.
        /// </summary>
        /// <param name="dropedfile">
        /// The dropedfile.
        /// </param>
        /// <param name="category_id">
        /// The category_id.
        /// </param>
        /// <param name="org_name">
        /// The org_name.
        /// </param>
        /// <param name="start_date">
        /// The start_date.
        /// </param>
        /// <param name="end_date">
        /// The end_date.
        /// </param>
        /// <param name="org_id">
        /// The org_id.
        /// </param>
        /// <param name="comments">
        /// The comments.
        /// </param>
        [HttpPost]
        public void Create_Doc_by_DDrop(
            HttpPostedFileBase dropedfile,
            int category_id,
            string org_name,
            string start_date,
            string end_date,
            int org_id,
            string comments)
        {
            var repository = new InstitutionalFilesRepo();

            try
            {
                if (dropedfile != null && dropedfile.ContentLength > 0)
                {
                    // get file name and file Extension
                    var fileName = Path.GetFileName(dropedfile.FileName);
                    var fileExtension = Path.GetExtension(fileName);

                    // get document id and create new document name 
                    var docID = repository.GetDocID(
                        org_id,
                        category_id,
                        fileExtension,
                        start_date,
                        end_date,
                        Convert.ToString(this.Session["ic"]),
                        Convert.ToString(this.Session["userid"]),
                        comments);

                    var docName = Convert.ToString(docID) + fileExtension;

                    // upload to image sever 
                    var fileFolder = @"\\" + Convert.ToString(this.Session["WebGrantUrl"]) + "\\egrants\\funded\\nci\\institutional\\";
                    var filePath = Path.Combine(fileFolder, docName);
                    dropedfile.SaveAs(filePath);

                }
                else
                {
                    this.ViewBag.Message = "You have not specified a file.";
                }
            }
            catch (Exception ex)
            {
                this.ViewBag.Message = "ERROR:" + ex.Message;
            }
        }

        /// <summary>
        /// The create_ doc_by_ file.
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="category_id">
        /// The category_id.
        /// </param>
        /// <param name="org_name">
        /// The org_name.
        /// </param>
        /// <param name="start_date">
        /// The start_date.
        /// </param>
        /// <param name="end_date">
        /// The end_date.
        /// </param>
        /// <param name="org_id">
        /// The org_id.
        /// </param>
        /// <param name="comments">
        /// The comments.
        /// </param>
        [HttpPost]
        public void Create_Doc_by_File(
            HttpPostedFileBase file,
            int category_id,
            string org_name,
            string start_date,
            string end_date,
            int org_id,
            string comments)
        {
            string url = null;
            string mssg = null;
            var repository = new InstitutionalFilesRepo();

            try
            {
                if (file != null && file.ContentLength > 0 && category_id != 0)
                {
                    // get file name and file Extension
                    var fileName = Path.GetFileName(file.FileName);
                    var fileExtension = Path.GetExtension(fileName);

                    // get document id and create new document name 
                    var docID = repository.GetDocID(
                        org_id,
                        category_id,
                        fileExtension,
                        start_date,
                        end_date,
                        Convert.ToString(this.Session["ic"]),
                        Convert.ToString(this.Session["userid"]),
                        comments);

                    var docName = Convert.ToString(docID) + fileExtension;

                    var fileFolder = @"\\" + Convert.ToString(this.Session["WebGrantUrl"]) + "\\egrants\\funded\\nci\\institutional\\";
                    var filePath = Path.Combine(fileFolder, docName);
                    file.SaveAs(filePath);
                }
                else
                {
                    this.ViewBag.Message = "You have not specified information correctly.";
                }
            }
            catch (Exception ex)
            {
                this.ViewBag.Message = "ERROR:" + ex.Message;
            }
        }

        /// <summary>
        /// The update_ doc.
        /// </summary>
        /// <param name="category_id">
        /// The category_id.
        /// </param>
        /// <param name="start_date">
        /// The start_date.
        /// </param>
        /// <param name="end_date">
        /// The end_date.
        /// </param>
        /// <param name="comments">
        /// The comments.
        /// </param>
        /// <param name="doc_id">
        /// The doc_id.
        /// </param>
        [HttpPost]
        public void Update_Doc(int category_id, string start_date, string end_date, string comments, int doc_id)
        {
            var repository = new InstitutionalFilesRepo();

            try
            {
                if (category_id != 0)
                    repository.UpdateDocument(
                        doc_id,
                        category_id,
                        start_date,
                        end_date,
                        Convert.ToString(this.Session["ic"]),
                        Convert.ToString(this.Session["userid"]),
                        comments);
                else
                    this.ViewBag.Message = "You have not specified information correctly.";
            }
            catch (Exception ex)
            {
                this.ViewBag.Message = "ERROR:" + ex.Message;
            }
        }
    }
}