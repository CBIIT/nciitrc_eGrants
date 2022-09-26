#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  InstitutionalDocFiles.cs
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
    /// The institutional doc files.
    /// </summary>
    public class InstitutionalDocFiles
    {
        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        public int Tag { get; set; }

        /// <summary>
        /// Gets or sets the org_id.
        /// </summary>
        public string org_id { get; set; }

        /// <summary>
        /// Gets or sets the org_name.
        /// </summary>
        public string org_name { get; set; }

        /// <summary>
        /// Gets or sets the document id.
        /// </summary>
        public int DocumentId { get; set; }

        /// <summary>
        /// Gets or sets the category_name.
        /// </summary>
        public string category_name { get; set; }

        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// Gets or sets the start_date.
        /// </summary>
        public string start_date { get; set; }

        /// <summary>
        /// Gets or sets the end_date.
        /// </summary>
        public string end_date { get; set; }

        /// <summary>
        /// Gets or sets the created_date.
        /// </summary>
        public string created_date { get; set; }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        public string comments { get; set; }
    }
}