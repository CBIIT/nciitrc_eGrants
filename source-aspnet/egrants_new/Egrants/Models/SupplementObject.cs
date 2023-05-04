#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  SupplementObject.cs
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
    ///     The supplement.
    /// </summary>
    public class SupplementObject
    {
        /// <summary>
        ///     Gets or sets the tag.
        /// </summary>
        public string tag { get; set; }

        /// <summary>
        ///     Gets or sets the grant_id.
        /// </summary>
        public string grant_id { get; set; }

        /// <summary>
        ///     Gets or sets the admin_phs_org_code.
        /// </summary>
        public string admin_phs_org_code { get; set; }

        /// <summary>
        ///     Gets or sets the serial_num.
        /// </summary>
        public string serial_num { get; set; }

        /// <summary>
        ///     Gets or sets the id.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        ///     Gets or sets the full_grant_num.
        /// </summary>
        public string full_grant_num { get; set; }

        /// <summary>
        ///     Gets or sets the supp_appl_id.
        /// </summary>
        public string supp_appl_id { get; set; }

        /// <summary>
        ///     Gets or sets the support_year.
        /// </summary>
        public string support_year { get; set; }

        /// <summary>
        ///     Gets or sets the suffix_code.
        /// </summary>
        public string suffix_code { get; set; }

        /// <summary>
        ///     Gets or sets the former_num.
        /// </summary>
        public string former_num { get; set; }

        /// <summary>
        ///     Gets or sets the submitted_date.
        /// </summary>
        public string submitted_date { get; set; }

        /// <summary>
        ///     Gets or sets the category_name.
        /// </summary>
        public string category_name { get; set; }

        /// <summary>
        ///     Gets or sets the status.
        /// </summary>
        public string status { get; set; }

        /// <summary>
        ///     Gets or sets the url.
        /// </summary>
        public string url { get; set; }

        /// <summary>
        ///     Gets or sets the moved_date.
        /// </summary>
        public string moved_date { get; set; }

        /// <summary>
        ///     Gets or sets the moved_by.
        /// </summary>
        public string moved_by { get; set; }
    }
}