#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  InstitutionalFilesPage.cs
// Solution: eGrants
// Project:  eGrants
// Created: 2025-10-22
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
using System.Collections.Generic;

using eGrants.Common.Enums;
using eGrants.DTOs;
using eGrants.Models;

#endregion

namespace eGrants.ViewModels
{
    /// <summary>
    /// The institutional files page.
    /// </summary>
    public class InstitutionalFilesPageViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InstitutionalFilesPageViewModel"/> class.
        /// </summary>
        public InstitutionalFilesPageViewModel()
        {
            this.OrgList = new List<InstFileFindOrgDTO>();
            this.DocFiles = new List<InstFileLoadOrgDocListDTO>();
            this.OrgCategories = new List<InstitutionalOrgCategory>();
            this.CharacterIndices = new List<InsitutionalOrgNameIndex>();
        }

        /// <summary>
        /// Gets or sets the selected institutional org.
        /// </summary>
        public InstFileFindOrgDTO SelectedInstitutionalOrg { get; set; }

        /// <summary>
        /// Gets or sets the org list.
        /// </summary>
        public List<InstFileFindOrgDTO> OrgList { get; set; }

        /// <summary>
        /// Gets or sets the doc files.
        /// </summary>
        public List<InstFileLoadOrgDocListDTO> DocFiles { get; set; }

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