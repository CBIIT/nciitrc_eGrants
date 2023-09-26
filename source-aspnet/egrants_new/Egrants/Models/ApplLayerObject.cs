#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  ApplLayerObject.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-12-02
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

using System.Collections.Generic;

namespace egrants_new.Egrants.Models
{

    /// <summary>
    ///     The appllayer.
    /// </summary>
    public class ApplLayerObject
    {
        /// <summary>
        ///     Gets or sets the grant_id.
        /// </summary>
        public string grant_id { get; set; }

        /// <summary>
        ///     Gets or sets the appl_id.
        /// </summary>
        public string appl_id { get; set; }

        /// <summary>
        ///     Gets or sets the full_grant_num.
        /// </summary>
        public string full_grant_num { get; set; }

        /// <summary>
        ///     Gets or sets the support_year.
        /// </summary>
        public string support_year { get; set; }

        /// <summary>
        ///     Gets or sets the appl_type_code.
        /// </summary>
        public string appl_type_code { get; set; }

        /// <summary>
        ///     Gets or sets the deleted_by_impac.
        /// </summary>
        public string deleted_by_impac { get; set; }

        /// <summary>
        ///     Gets or sets the doc_count.
        /// </summary>
        public string doc_count { get; set; }

        /// <summary>
        ///     Gets or sets the closeout_notcount.
        /// </summary>
        public string closeout_notcount { get; set; }

        /// <summary>
        ///     Gets or sets the can_add_doc.
        /// </summary>
        public string can_add_doc { get; set; }

        /// <summary>
        ///     Gets or sets the competing.
        /// </summary>
        public string competing { get; set; }

        /// <summary>
        ///     Gets or sets the fsr_count.
        /// </summary>
        public string fsr_count { get; set; }

        /// <summary>
        ///     Gets or sets the frc_destroyed.
        /// </summary>
        public string frc_destroyed { get; set; }

        /// <summary>
        ///     Gets or sets the appl_fda_flag.
        /// </summary>
        public string appl_fda_flag { get; set; }

        /// <summary>
        ///     Gets or sets the appl_ms_flag.
        /// </summary>
        public string appl_ms_flag { get; set; }

        /// <summary>
        ///     Gets or sets the appl_od_flag.
        /// </summary>
        public string appl_od_flag { get; set; }

        /// <summary>
        ///     Gets or sets the appl_ds_flag.
        /// </summary>
        public string appl_ds_flag { get; set; }

        /// <summary>
        ///     Gets or sets the closeout_flag.
        /// </summary>
        public string closeout_flag { get; set; }

        /// <summary>
        ///     Gets or sets the irppr_id.
        /// </summary>
        public string irppr_id { get; set; }

        /// <summary>
        ///     Gets or sets the can_add_funding.
        /// </summary>
        public string can_add_funding { get; set; }

        /// <summary>
        ///     Gets or sets the label.
        /// </summary>
        public string label { get; set; }

        /// <summary>
        /// Gets or sets the MPI contact info.
        /// </summary>
        public List<PersonContact> MPIContacts { get; set; }

        //   public List<ImpacDocs> ImpacDocsList { get; set; }
    }
}
