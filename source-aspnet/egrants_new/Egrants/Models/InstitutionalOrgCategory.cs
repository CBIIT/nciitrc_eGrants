#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  InstitutionalOrgCategory.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-05-17
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
    /// The institutional org category.
    /// </summary>
    public class InstitutionalOrgCategory
    {
        /// <summary>
        /// Gets or sets the category_id.
        /// </summary>
        public string category_id { get; set; }

        /// <summary>
        /// Gets or sets the category_name.
        /// </summary>
        public string category_name { get; set; }

        /// <summary>
        /// Gets or sets the tobe_flag.
        /// </summary>
        public string tobe_flag { get; set; }

        /// <summary>
        /// Gets or sets the flag_period.
        /// </summary>
        public string flag_period { get; set; }

        /// <summary>
        /// Gets or sets the flag_data.
        /// </summary>
        public string flag_data { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether require_comments.
        /// </summary>
        public bool require_comments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether active.
        /// </summary>
        public bool active { get; set; }
    }
}