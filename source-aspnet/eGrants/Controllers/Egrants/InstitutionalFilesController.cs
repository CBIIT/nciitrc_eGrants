#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  InstitutionalFilesController.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2025-10-22
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Web;

using eGrants.Common.Enums;
using eGrants.Models;
using eGrants.Repositories.Interfaces;
using eGrants.Services.Interfaces;
using eGrants.ViewModels;

using Microsoft.AspNetCore.Mvc;

#endregion

namespace eGrant.Controllers
{
    /// <summary>
    /// The institutional files controller.
    /// </summary>
    public class InstitutionalFilesController : Controller
    {
        private readonly IInstitutionalFilesService _institutionalFilesService;
        private readonly IInstitutionalFilesRepository _institutionalFilesRepository;
        private readonly ISessionInfoService _sessionInfoService;

        private SessionInfo sessionInfo => _sessionInfoService.GetSessionInfo(HttpContext.Session);

        public InstitutionalFilesController(IInstitutionalFilesService institutionalFilesService, IInstitutionalFilesRepository institutionalFilesRepository, ISessionInfoService sessionInfoService)
        {
            _institutionalFilesService = institutionalFilesService;
            _institutionalFilesRepository = institutionalFilesRepository;
            _sessionInfoService = sessionInfoService;
        }
        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            //var repository = new InstitutionalFilesRepo();

            // Create new Page Model to adhere to MVC practices
            // Should have a Builder but... This will do for now
            var page = new InstitutionalFilesPage
            {
                SelectedInstitutionalOrg = new InstitutionalOrg(),
                Action = InstitutionalFilesPageAction.ShowOrgs,
                CharacterIndices = await _institutionalFilesRepository.LoadOrgNameCharacterIndices(),
                OrgList = await _institutionalFilesRepository.LoadOrgList(2)
            };

            return View("~/Views/eGrants/InstitutionalFilesIndex.cshtml", page);
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
        public async Task<ActionResult> Show_Orgs(int index_id)
        {
            var page = new InstitutionalFilesPage
            {
                SelectedInstitutionalOrg = new InstitutionalOrg(),
                Action = InstitutionalFilesPageAction.ShowOrgs,
                CharacterIndices = await _institutionalFilesService.LoadOrgNameCharacterIndices(),
                OrgList = await _institutionalFilesService.LoadOrgList(index_id)
            };

            return this.View("~/Views/eGrants/InstitutionalFilesIndex.cshtml", page);
        }

        ///// <summary>
        ///// The search_ orgs.
        ///// </summary>
        ///// <param name="str">
        ///// The str.
        ///// </param>
        ///// <returns>
        ///// The <see cref="ActionResult"/>.
        ///// </returns>
        //[HttpGet]
        //public ActionResult Search_Orgs(string str)
        //{
        //    var repository = new InstitutionalFilesRepo();

        //    var page = new InstitutionalFilesPage
        //                   {
        //                       SelectedInstitutionalOrg = new InstitutionalOrg(),
        //                       Action = InstitutionalFilesPageAction.ShowOrgs,
        //                       CharacterIndices = repository.LoadOrgNameCharacterIndices(),
        //                       OrgList = repository.SearchOrgList(str)
        //                   };

        //    return this.View("~/Egrants/Views/InstitutionalFilesIndex.cshtml", page);
        //}

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
        public async Task<ActionResult> Show_Docs(int org_id = 0, string org_name = "")
        {
            //var repository = new InstitutionalFilesRepo();

            var selectedInstitutionalOrg = await _institutionalFilesService.FindOrg(org_id, org_name);

            var page = new InstitutionalFilesPage
            {
                SelectedInstitutionalOrg = selectedInstitutionalOrg,
                Action = InstitutionalFilesPageAction.ShowDocs,
                CharacterIndices = await _institutionalFilesService.LoadOrgNameCharacterIndices(),
                DocFiles = await _institutionalFilesService.LoadOrgDocList(selectedInstitutionalOrg.OrgId)
            };

            return View("~/Views/eGrants/InstitutionalFilesIndex.cshtml", page);
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
        public async Task<ActionResult> Delete_Doc(string act, int doc_id, int org_id, string org_name)
        {
            // disable_doc
            _institutionalFilesService.DisableDoc(doc_id, sessionInfo.UserId);

            this.ViewBag.Act = act;
            this.ViewBag.OrgID = org_id;
            this.ViewBag.OrgName = org_name;

            return await Show_Docs(org_id, org_name);
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
        public async Task<ActionResult> Show_Create_Doc(int org_id)
        {
            // set act
            var selectedInstitutionalOrg = await _institutionalFilesService.FindOrg(org_id);

            var page = new InstitutionalFilesPage
            {
                SelectedInstitutionalOrg = selectedInstitutionalOrg,
                Action = InstitutionalFilesPageAction.CreateNew,
                CharacterIndices = await _institutionalFilesService.LoadOrgNameCharacterIndices(),
                DocFiles = await _institutionalFilesService.LoadOrgDocList(org_id),
                OrgCategories = await _institutionalFilesService.LoadOrgCategory(true),
                TodayText = DateTime.Now.ToShortDateString()
            };

            return this.View("~/Views/eGrants/InstitutionalFilesIndex.cshtml", page);
        }

        /// <summary>
        /// The show_ update_ doc.
        /// </summary>
        /// <param name="docId">
        /// The doc_id.
        /// </param>
        /// <param name="orgId">
        /// The org_id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpGet]
        public async Task<ActionResult> Show_Update_Doc(int doc_id, int org_id)
        {
            var selectedInstitutionalOrg = await _institutionalFilesService.FindOrg(org_id);

            var docDto = (await _institutionalFilesService.LoadOrgDocList(selectedInstitutionalOrg.OrgId)).Where(d => d.DocumentId == doc_id).FirstOrDefault();

            var page = new InstitutionalFilesPage
            {
                SelectedInstitutionalOrg = selectedInstitutionalOrg,
                Action = InstitutionalFilesPageAction.UpdateDoc,
                CharacterIndices = await _institutionalFilesService.LoadOrgNameCharacterIndices(),
                SelectedDocFile = docDto == null ? null : new InstitutionalDocFiles
                {
                    DocumentId = docDto.DocumentId,
                    category_name = docDto.category_name,
                    created_date = docDto.created_date,
                    org_id = docDto.org_id.ToString(),
                    org_name = docDto.org_name,
                    url = docDto.url,
                    start_date = docDto.start_date,
                    end_date = docDto.end_date,
                    comments = docDto.comments
                    // Map other properties as needed
                },
                OrgCategories = await _institutionalFilesService.LoadOrgCategory(false),
                TodayText = DateTime.Now.ToShortDateString()
            };

            return View("~/Views/eGrants/InstitutionalFilesIndex.cshtml", page);
        }

        ///// <summary>
        ///// The create_ doc_by_ d drop.
        ///// </summary>
        ///// <param name="dropedfile">
        ///// The dropedfile.
        ///// </param>
        ///// <param name="category_id">
        ///// The category_id.
        ///// </param>
        ///// <param name="org_name">
        ///// The org_name.
        ///// </param>
        ///// <param name="start_date">
        ///// The start_date.
        ///// </param>
        ///// <param name="end_date">
        ///// The end_date.
        ///// </param>
        ///// <param name="org_id">
        ///// The org_id.
        ///// </param>
        ///// <param name="comments">
        ///// The comments.
        ///// </param>
        //[HttpPost]
        //public void Create_Doc_by_DDrop(
        //    HttpPostedFileBase dropedfile,
        //    int category_id,
        //    string org_name,
        //    string start_date,
        //    string end_date,
        //    int org_id,
        //    string comments)
        //{
        //    var repository = new InstitutionalFilesRepo();

        //    try
        //    {
        //        if (dropedfile != null && dropedfile.ContentLength > 0)
        //        {
        //            // get file name and file Extension
        //            var fileName = Path.GetFileName(dropedfile.FileName);
        //            var fileExtension = Path.GetExtension(fileName);

        //            // get document id and create new document name 
        //            var docID = repository.GetDocID(
        //                org_id,
        //                category_id,
        //                fileExtension,
        //                start_date,
        //                end_date,
        //                Convert.ToString(this.Session["ic"]),
        //                Convert.ToString(this.Session["userid"]),
        //                comments);

        //            var docName = Convert.ToString(docID) + fileExtension;

        //            // upload to image sever 
        //            var fileFolder = @"\\" + Convert.ToString(this.Session["WebGrantUrl"]) + "\\egrants\\funded\\nci\\institutional\\";
        //            var filePath = Path.Combine(fileFolder, docName);
        //            dropedfile.SaveAs(filePath);

        //        }
        //        else
        //        {
        //            this.ViewBag.Message = "You have not specified a file.";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        this.ViewBag.Message = "ERROR:" + ex.Message;
        //    }
        //}

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
        public async Task Create_Doc_by_File(
            IFormFile file,
            int category_id,
            string org_name,
            string start_date,
            string end_date,
            int org_id,
            string comments)
        {
            string url = null;
            string mssg = null;

            try
            {
                if (file != null && file.Length > 0 && category_id != 0)
                {
                    // get file name and file Extension
                    var fileName = Path.GetFileName(file.FileName);
                    var fileExtension = Path.GetExtension(fileName);

                    // get document id and create new document name 
                    var docID = _institutionalFilesService.GetDocID(
                        org_id,
                        category_id,
                        fileExtension,
                        start_date ?? "",
                        end_date ?? "",
                        sessionInfo.Ic,
                        sessionInfo.UserId,
                        comments ?? "");

                    var docName = Convert.ToString(docID) + fileExtension;

                    var fileFolder = @"\\" + sessionInfo.WebGrantUrl + "\\egrants\\funded\\nci\\institutional\\";
                    var filePath = Path.Combine(fileFolder, docName);
                    // save file asynchronously
                    await using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);
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
        public async Task Update_Doc(int category_id, string start_date, string end_date, string comments, int doc_id)
        {
            try
            {
                if (category_id != 0)
                    _institutionalFilesService.UpdateDocument(
                        doc_id,
                        category_id,
                        start_date ?? "",
                        end_date ?? "",
                        sessionInfo.Ic,
                        sessionInfo.UserId,
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