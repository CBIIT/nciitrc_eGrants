#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  SQLJobError.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-03-31
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

#endregion

namespace egrants_new.Integration.Models
{
    /// <summary>
    /// The sql job error.
    /// </summary>
    public class SQLJobError
    {
        /// <summary>
        /// The email sent.
        /// </summary>
        public bool EmailSent;

        /// <summary>
        /// The error date time.
        /// </summary>
        public DateTime ErrorDateTime;

        /// <summary>
        /// The error id.
        /// </summary>
        public int ErrorId;

        /// <summary>
        /// The error line.
        /// </summary>
        public int ErrorLine;

        /// <summary>
        /// The error message.
        /// </summary>
        public string ErrorMessage;

        /// <summary>
        /// The error number.
        /// </summary>
        public int ErrorNumber;

        /// <summary>
        /// The error procedure.
        /// </summary>
        public string ErrorProcedure;

        /// <summary>
        /// The error severity.
        /// </summary>
        public int ErrorSeverity;

        /// <summary>
        /// The error state.
        /// </summary>
        public int ErrorState;

        /// <summary>
        /// The job id.
        /// </summary>
        public string JobId;

        /// <summary>
        /// The job name.
        /// </summary>
        public string JobName;

        /// <summary>
        /// The step id.
        /// </summary>
        public int StepId;

        /// <summary>
        /// The user name.
        /// </summary>
        public string UserName;
    }
}