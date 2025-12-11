#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  DownloadData.cs
// Solution: eGrants
// Project:  eGrants
// Created: 2025-12-11
// Contributors:
//      - Feroz, Aalyaan (NIH/NCI) [C] - feroza2
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

namespace eGrants.Models
{
    /// <summary>
    /// Individual download data for a file
    /// </summary>
    public class DownloadData
    {
        /// <summary>
        /// Gets or sets the URL of the file
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the downloaded filename
        /// </summary>
        public string FileDownloaded { get; set; }

        /// <summary>
        /// Gets or sets the category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the subcategory
        /// </summary>
        public string SubCategory { get; set; }

        /// <summary>
        /// Gets or sets the document name
        /// </summary>
        public string DocumentName { get; set; }

        /// <summary>
        /// Gets or sets the document date
        /// </summary>
        public DateTime? DocumentDate { get; set; }

        /// <summary>
        /// Gets or sets the document ID
        /// </summary>
        public int DocumentId { get; set; }

        /// <summary>
        /// Gets or sets any error that occurred during download
        /// </summary>
        public string Error { get; set; }
    }
}