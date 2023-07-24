#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  DocumentsObject.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2023-07-20
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

#endregion

namespace egrants_new.Models
{
    /// <summary>
    /// The documents object.
    /// </summary>
    public class DocumentsObject
    {
        /// <summary>
        /// Gets or sets the document_id.
        /// </summary>
        private int document_id { get; set; }

        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        private string url { get; set; }

        /// <summary>
        /// Gets or sets the page_count.
        /// </summary>
        private int page_count { get; set; }

        /// <summary>
        /// Gets or sets the appl_id.
        /// </summary>
        private int appl_id { get; set; }

        /// <summary>
        /// Gets or sets the stamp.
        /// </summary>
        private string stamp { get; set; }

        /// <summary>
        /// Gets or sets the category_id.
        /// </summary>
        private int category_id { get; set; }

        /// <summary>
        /// Gets or sets the document_size.
        /// </summary>
        private int document_size { get; set; }

        /// <summary>
        /// Gets or sets the problem_msg.
        /// </summary>
        private string problem_msg { get; set; }

        /// <summary>
        /// Gets or sets the file_type.
        /// </summary>
        private string file_type { get; set; }

        /// <summary>
        /// Gets or sets the profile_id.
        /// </summary>
        private int profile_id { get; set; }

        /// <summary>
        /// Gets or sets the corrupted.
        /// </summary>
        private string corrupted { get; set; }

        /// <summary>
        /// Gets or sets the alias.
        /// </summary>
        private string alias { get; set; }

        /// <summary>
        /// Gets or sets the inventoried.
        /// </summary>
        private string inventoried { get; set; }

        /// <summary>
        /// Gets or sets the qc_reason.
        /// </summary>
        private string qc_reason { get; set; }

        /// <summary>
        /// Gets or sets the document_date.
        /// </summary>
        private DateTime document_date { get; set; }

        /// <summary>
        /// Gets or sets the created_date.
        /// </summary>
        private DateTime created_date { get; set; }

        /// <summary>
        /// Gets or sets the modified_date.
        /// </summary>
        private DateTime modified_date { get; set; }

        /// <summary>
        /// Gets or sets the updated_date.
        /// </summary>
        private DateTime updated_date { get; set; }

        /// <summary>
        /// Gets or sets the stored_date.
        /// </summary>
        private DateTime stored_date { get; set; }

        /// <summary>
        /// Gets or sets the file_modified_date.
        /// </summary>
        private DateTime file_modified_date { get; set; }

        /// <summary>
        /// Gets or sets the qc_date.
        /// </summary>
        private DateTime qc_date { get; set; }

        /// <summary>
        /// Gets or sets the disabled_date.
        /// </summary>
        private DateTime disabled_date { get; set; }

        /// <summary>
        /// Gets or sets the qc_person_id.
        /// </summary>
        private int qc_person_id { get; set; }

        /// <summary>
        /// Gets or sets the created_by_person_id.
        /// </summary>
        private int created_by_person_id { get; set; }

        /// <summary>
        /// Gets or sets the stored_by_person_id.
        /// </summary>
        private int stored_by_person_id { get; set; }

        /// <summary>
        /// Gets or sets the index_modified_by_person_id.
        /// </summary>
        private int index_modified_by_person_id { get; set; }

        /// <summary>
        /// Gets or sets the file_modified_by_person_id.
        /// </summary>
        private int file_modified_by_person_id { get; set; }

        /// <summary>
        /// Gets or sets the disabled_by_person_id.
        /// </summary>
        private int disabled_by_person_id { get; set; }

        /// <summary>
        /// Gets or sets the problem_reported_by_person_id.
        /// </summary>
        private int problem_reported_by_person_id { get; set; }

        /// <summary>
        /// Gets or sets the nga_id.
        /// </summary>
        private int nga_id { get; set; }

        /// <summary>
        /// Gets or sets the external_upload__id.
        /// </summary>
        private int external_upload__id { get; set; }

        /// <summary>
        /// Gets or sets the mail_upload_id.
        /// </summary>
        private string mail_upload_id { get; set; }

        /// <summary>
        /// Gets or sets the status_id.
        /// </summary>
        private int status_id { get; set; }

        /// <summary>
        /// Gets or sets the uid.
        /// </summary>
        private string uid { get; set; }

        /// <summary>
        /// Gets or sets the aws_id.
        /// </summary>
        private int aws_id { get; set; }

        /// <summary>
        /// Gets or sets the impp_doc_id.
        /// </summary>
        private int impp_doc_id { get; set; }

        /// <summary>
        /// Gets or sets the processed_date.
        /// </summary>
        private DateTime processed_date { get; set; }

        /// <summary>
        /// Gets or sets the nga_rpt_seq_num.
        /// </summary>
        private int nga_rpt_seq_num { get; set; }

        /// <summary>
        /// Gets or sets the is_destoryed.
        /// </summary>
        private int is_destoryed { get; set; }

        /// <summary>
        /// Gets or sets the control_id.
        /// </summary>
        private int control_id { get; set; }

        /// <summary>
        /// Gets or sets the parent_id.
        /// </summary>
        private int parent_id { get; set; }

        /// <summary>
        /// Gets or sets the sub_category_name.
        /// </summary>
        private string sub_category_name { get; set; }
    }
}