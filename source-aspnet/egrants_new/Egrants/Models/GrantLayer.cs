// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GrantLayer.cs" company="">
//   
// </copyright>
// <summary>
//   The grantlayer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  GrantLayer.cs
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

namespace egrants_new.Egrants.Models
{

    /// <summary>
    ///     The grantlayer.
    /// </summary>
    public class GrantLayer
    {
        /// <summary>
        ///     Gets or sets the grant_id.
        /// </summary>
        public string grant_id { get; set; }

        /// <summary>
        ///     Gets or sets the org_name.
        /// </summary>
        public string org_name { get; set; }

        /// <summary>
        ///     Gets or sets the admin_phs_org_code.
        /// </summary>
        public string admin_phs_org_code { get; set; }

        /// <summary>
        ///     Gets or sets the serial_num.
        /// </summary>
        public string serial_num { get; set; }

        /// <summary>
        ///     Gets or sets the grant_num.
        /// </summary>
        public string grant_num { get; set; }

        /// <summary>
        ///     Gets or sets the former_grant_num.
        /// </summary>
        public string former_grant_num { get; set; }

        /// <summary>
        ///     Gets or sets the latest_full_grant_num.
        /// </summary>
        public string latest_full_grant_num { get; set; }

        /// <summary>
        ///     Gets or sets the all_activity_code.
        /// </summary>
        public string all_activity_code { get; set; }

        /// <summary>
        ///     Gets or sets the project_title.
        /// </summary>
        public string project_title { get; set; }

        /// <summary>
        ///     Gets or sets the pi_name.
        /// </summary>
        public string pi_name { get; set; }

        /// <summary>
        ///     Gets or sets the current_pi_name.
        /// </summary>
        public string current_pi_name { get; set; }

        /// <summary>
        ///     Gets or sets the current_pi_email_address.
        /// </summary>
        public string current_pi_email_address { get; set; }

        /// <summary>
        ///     Gets or sets the current_pd_name.
        /// </summary>
        public string current_pd_name { get; set; }

        /// <summary>
        ///     Gets or sets the current_pd_email_address.
        /// </summary>
        public string current_pd_email_address { get; set; }

        /// <summary>
        ///     Gets or sets the current_spec_name.
        /// </summary>
        public string current_spec_name { get; set; }

        /// <summary>
        ///     Gets or sets the current_spec_email_address.
        /// </summary>
        public string current_spec_email_address { get; set; }

        /// <summary>
        ///     Gets or sets the current_bo_email_address.
        /// </summary>
        public string current_bo_email_address { get; set; }

        /// <summary>
        ///     Gets or sets the prog_class_code.
        /// </summary>
        public string prog_class_code { get; set; }

        /// <summary>
        ///     Gets or sets the sv_url.
        /// </summary>
        public string sv_url { get; set; }

        /// <summary>
        ///     Gets or sets the arra_flag.
        /// </summary>
        public string arra_flag { get; set; }

        /// <summary>
        ///     Gets or sets the fda_flag.
        /// </summary>
        public string fda_flag { get; set; }

        /// <summary>
        ///     Gets or sets the stop_flag.
        /// </summary>
        public string stop_flag { get; set; }

        /// <summary>
        ///     Gets or sets the ms_flag.
        /// </summary>
        public string ms_flag { get; set; }

        /// <summary>
        ///     Gets or sets the od_flag.
        /// </summary>
        public string od_flag { get; set; }

        /// <summary>
        ///     Gets or sets the ds_flag.
        /// </summary>
        public string ds_flag { get; set; }

        /// <summary>
        ///     Gets or sets the adm_supp.
        /// </summary>
        public string adm_supp { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether institutional_flag 1.
        /// </summary>
        public bool institutional_flag1 { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether institutional_flag 2.
        /// </summary>
        public bool institutional_flag2 { get; set; }

        /// <summary>
        ///     Gets or sets the inst_flag 1_url.
        /// </summary>
        public string inst_flag1_url { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether any org doc.
        /// </summary>
        public bool AnyOrgDoc { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether show email hyper link.
        /// </summary>
        public bool ShowEmailHyperLink { get; set; }

        /// <summary>
        ///     Gets or sets the selected grant pi first name.
        /// </summary>
        public string SelectedGrantPiFirstName { get; set; }

        /// <summary>
        ///     Gets or sets the selected grant pi lastname.
        /// </summary>
        public string SelectedGrantPiLastname { get; set; }

        /// <summary>
        ///     Gets or sets the selected grant pi middle initial.
        /// </summary>
        public string SelectedGrantPiMiddleInitial { get; set; }

        /// <summary>
        ///     Gets or sets the selected organization name.
        /// </summary>
        public string SelectedOrganizationName { get; set; }

        /// <summary>
        /// Gets or sets the program directory full name.
        /// </summary>
        public string ProgramDirectorFullName { get; set; }

        /// <summary>
        /// Gets or sets the program director email address.
        /// </summary>
        public string ProgramDirectorEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the specialist full name.
        /// </summary>
        public string SpecialistFullName { get; set; }

        /// <summary>
        /// Gets or sets the specialist email address.
        /// </summary>
        public string SpecialistEmailAddress { get; set; }
    }
}