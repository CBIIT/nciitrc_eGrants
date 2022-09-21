#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  EgrantsCommon.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-05-05
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


#endregion

namespace egrants_new.Models
{

    /// <summary>
    ///     The user.
    /// </summary>
    public class User
    {
        /// <summary>
        ///     Gets or sets the person id.
        /// </summary>
        public int personID { get; set; }

        /// <summary>
        ///     Gets or sets the position id.
        /// </summary>
        public int positionID { get; set; }

        /// <summary>
        ///     Gets or sets the is coordinator.
        /// </summary>
        public int isCoordinator { get; set; }

        /// <summary>
        ///     Gets or sets the person name.
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        ///     Gets or sets the user id.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        ///     Gets or sets the person email.
        /// </summary>
        public string PersonEmail { get; set; }

        /// <summary>
        ///     Gets or sets the validation.
        /// </summary>
        public string Validation { get; set; }

        /// <summary>
        ///     Gets or sets the menulist.
        /// </summary>
        public string menulist { get; set; }

        /// <summary>
        ///     Gets or sets the ic.
        /// </summary>
        public string ic { get; set; }
    }
}