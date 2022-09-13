#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  InstitutionalFilesPage.cs
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

#region

using System;
using System.Collections.Generic;

#endregion

namespace egrants_new.Egrants.Models
{
    /// <summary>
    /// The institutional files page.
    /// </summary>
    public class InstitutionalFilesPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InstitutionalFilesPage"/> class.
        /// </summary>
        public InstitutionalFilesPage()
        {
            this.OrgList = new List<InstitutionalOrg>();
            this.DocFiles = new List<InstitutionalDocFiles>();
            this.OrgCategories = new List<InstitutionalOrgCategory>();
            this.CharacterIndices = new List<InsitutionalOrgNameIndex>();
        }

        /// <summary>
        /// Gets or sets the selected institutional org.
        /// </summary>
        public InstitutionalOrg SelectedInstitutionalOrg { get; set; }

        /// <summary>
        /// Gets or sets the org list.
        /// </summary>
        public List<InstitutionalOrg> OrgList { get; set; }

        /// <summary>
        /// Gets or sets the doc files.
        /// </summary>
        public List<InstitutionalDocFiles> DocFiles { get; set; }

        /// <summary>
        /// Gets or sets the selected doc file.
        /// </summary>
        public InstitutionalDocFiles SelectedDocFile { get; set; }

        /// <summary>
        /// Gets or sets the org categories.
        /// </summary>
        public List<InstitutionalOrgCategory> OrgCategories { get; set; }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        public InstitutionalFilesPageAction Action { get; set; }

        /// <summary>
        /// Gets or sets the selected character index.
        /// </summary>
        public InsitutionalOrgNameIndex SelectedCharacterIndex { get; set; }

        /// <summary>
        /// Gets or sets the character indices.
        /// </summary>
        public List<InsitutionalOrgNameIndex> CharacterIndices { get; set; }

        /// <summary>
        /// Gets or sets the today.
        /// </summary>
        public DateTime Today { get; set; }

        /// <summary>
        /// Gets or sets the today text.
        /// </summary>
        public string TodayText { get; set; }
    }
}