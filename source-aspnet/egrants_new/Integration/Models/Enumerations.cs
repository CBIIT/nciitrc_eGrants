#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  Enumerations.cs
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

namespace egrants_new.Integration.Models
{
    /// <summary>
    /// The enumerations.
    /// </summary>
    public class Enumerations
    {
        /// <summary>
        /// The authentication type.
        /// </summary>
        public enum AuthenticationType
        {
            /// <summary>
            /// The user password.
            /// </summary>
            UserPassword = 0,

            /// <summary>
            /// The certificate.
            /// </summary>
            Certificate,

            /// <summary>
            /// The o auth.
            /// </summary>
            OAuth
        }

        /// <summary>
        /// The date time units.
        /// </summary>
        public enum DateTimeUnits
        {
            /// <summary>
            /// The seconds.
            /// </summary>
            Seconds = 0,

            /// <summary>
            /// The minutes.
            /// </summary>
            Minutes,

            /// <summary>
            /// The hours.
            /// </summary>
            Hours,

            /// <summary>
            /// The days.
            /// </summary>
            Days,

            /// <summary>
            /// The weeks.
            /// </summary>
            Weeks,

            /// <summary>
            /// The months.
            /// </summary>
            Months,

            /// <summary>
            /// The years.
            /// </summary>
            Years
        }

        /// <summary>
        /// The interval.
        /// </summary>
        public enum Interval
        {
            /// <summary>
            /// The interval.
            /// </summary>
            Interval = 0,

            /// <summary>
            /// The at set date time.
            /// </summary>
            AtSetDateTime

        }

        /// <summary>
        /// The reconciliation behavior.
        /// </summary>
        public enum ReconciliationBehavior
        {
            /// <summary>
            /// The add all duplication ok.
            /// </summary>
            AddAllDuplicationOk = 0, // This will work for the first integration because we'll ony be querying for most recent records by date

            /// <summary>
            /// The add new only.
            /// </summary>
            AddNewOnly, // Implement Later

            /// <summary>
            /// The add new update existing.
            /// </summary>
            AddNewUpdateExisting, // Implement Later 

            /// <summary>
            /// The update only reject new.
            /// </summary>
            UpdateOnlyRejectNew // Possibly Implement Later
        }
    }
}