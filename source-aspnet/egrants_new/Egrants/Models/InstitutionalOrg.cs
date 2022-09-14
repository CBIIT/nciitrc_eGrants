#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  InstitutionalOrg.cs
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
    /// The institutional org.
    /// </summary>
    public class InstitutionalOrg
    {
        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        public int Tag { get; set; }

        /// <summary>
        /// Gets or sets the org id.
        /// </summary>
        public int OrgId { get; set; }

        /// <summary>
        /// Gets or sets the org name.
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// Gets or sets the sv created by.
        /// </summary>
        public string SVCreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the sv created date.
        /// </summary>
        public string SVCreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the sv end date.
        /// </summary>
        public string SVEndDate { get; set; }

        /// <summary>
        /// Gets or sets the sv url.
        /// </summary>
        public string SvUrl { get; set; }

        /// <summary>
        /// Gets or sets the fu created date.
        /// </summary>
        public string FUCreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the fu created by.
        /// </summary>
        public string FUCreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the fu end date.
        /// </summary>
        public string FUEndDate { get; set; }

        /// <summary>
        /// Gets or sets the fu url.
        /// </summary>
        public string FUUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether any org doc.
        /// </summary>
        public bool AnyOrgDoc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether active.
        /// </summary>
        public bool Active { get; set; }
    }
}