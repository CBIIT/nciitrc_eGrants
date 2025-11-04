#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  doclayer.cs
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

using System;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace eGrants.Models
{
    /// <summary>
    ///     The doclayer.
    /// </summary>
    public class doclayer
    {
        /// <summary>
        ///     Gets or sets the appl_id.
        /// </summary>
        public int? appl_id { get; set; }

        /// <summary>
        ///     Gets or sets the docs_count.
        /// </summary>
        [NotMapped]
        public string docs_count { get; set; }

        /// <summary>
        ///     Gets or sets the grant_id.
        /// </summary>
        public int? grant_id { get; set; }

        /// <summary>
        ///     Gets or sets the full_grant_num.
        /// </summary>
        [NotMapped]
        public string full_grant_num { get; set; }

        /// <summary>
        ///     Gets or sets the document_id.
        /// </summary>
        public int? document_id { get; set; }

        /// <summary>
        ///     Gets or sets the document_name.
        /// </summary>
        public string? document_name { get; set; }

        /// <summary>
        ///     Gets or sets the category_id.
        /// </summary>
        public short? category_id { get; set; }

        /// <summary>
        ///     Gets or sets the category_name.
        /// </summary>
        public string? category_name { get; set; }

        /// <summary>
        ///     Gets or sets the sub_category_name.
        /// </summary>
        public string? sub_category_name { get; set; }

        /// <summary>
        ///     Gets or sets the created_by.
        /// </summary>
        public string? created_by { get; set; }


        /// <summary>
        ///     Gets or sets the modified_by.
        /// </summary>
        public string? modified_by { get; set; }



        /// <summary>
        ///     Gets or sets the file_modified_by.
        /// </summary>
        public string? file_modified_by { get; set; }



        /// <summary>
        ///     Gets or sets the problem_msg.
        /// </summary>
        public string? problem_msg { get; set; }

        /// <summary>
        ///     Gets or sets the problem_reported_by.
        /// </summary>
        public string? problem_reported_by { get; set; }

        /// <summary>
        ///     Gets or sets the page_count.
        /// </summary>
        public short? page_count { get; set; }

        /// <summary>
        ///     Gets or sets the fsr_count.
        /// </summary>
        public int? fsr_count { get; set; }

        /// <summary>
        ///     Gets or sets the attachment_count.
        /// </summary>
        public int? attachment_count { get; set; }

        /// <summary>
        ///     Gets or sets the closeout_notcount.
        /// </summary>
        public int? closeout_notcount { get; set; }

        /// <summary>
        ///     Gets or sets the frc_destroyed.
        /// </summary>
        public int? frc_destroyed { get; set; }

        /// <summary>
        ///     Gets or sets the url.
        /// </summary>
        public string? url { get; set; }



        /// <summary>
        ///     Gets or sets the can_qc.
        /// </summary>
        public string? can_qc { get; set; }

        /// <summary>
        ///     Gets or sets the can_upload.
        /// </summary>
        public string? can_upload { get; set; }

        /// <summary>
        ///     Gets or sets the can_modify_index.
        /// </summary>
        public string? can_modify_index { get; set; }

        /// <summary>
        ///     Gets or sets the can_delete.
        /// </summary>
        public string? can_delete { get; set; }

        /// <summary>
        ///     Gets or sets the can_restore.
        /// </summary>
        public string? can_restore { get; set; }

        /// <summary>
        ///     Gets or sets the can_store.
        /// </summary>
        public string? can_store { get; set; }

        /// <summary>
        ///     Gets or sets the ic.
        /// </summary>
        public string? ic { get; set; }

        /// <summary>
        ///     Gets or sets the created_date.
        /// </summary>
        public string? created_date { get; set; }

        /// <summary>
        ///     Gets or sets the document_date.
        /// </summary>
        public string? document_date { get; set; }

        /// <summary>
        ///     Gets or sets the doc_date.
        /// </summary>
        public DateTime? doc_date { get; set; }

        /// <summary>
        ///     Gets or sets the modified_date.
        /// </summary>
        public string? modified_date { get; set; }

        /// <summary>
        ///     Gets or sets the file_modified_date.
        /// </summary>
        public string? file_modified_date { get; set; }

        /// <summary>
        ///     Gets or sets the qc_date.
        /// </summary>
        public string? qc_date { get; set; }
    }
}