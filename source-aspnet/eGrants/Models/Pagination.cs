#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  Pagination.cs
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

namespace eGrants.Models
{
    /// <summary>
    ///     The pagination.
    /// </summary>
    public class Pagination
    {
        /// <summary>
        ///     Gets or sets the tag.
        /// </summary>
        public int? tag { get; set; }

        /// <summary>
        ///     Gets or sets the parent.
        /// </summary>
        public int? parent { get; set; }

        /// <summary>
        ///     Gets or sets the total_grants.
        /// </summary>
        public int? total_grants { get; set; }

        /// <summary>
        ///     Gets or sets the total_tabs.
        /// </summary>
        public int? total_tabs { get; set; }

        /// <summary>
        ///     Gets or sets the total_pages.
        /// </summary>
        public int? total_pages { get; set; }

        /// <summary>
        ///     Gets or sets the tab_number.
        /// </summary>
        public int? tab_number { get; set; }

        /// <summary>
        ///     Gets or sets the page_number.
        /// </summary>
        public int? page_number { get; set; }
    }
}