#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  WebServiceHistory.cs
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

using System;
using System.Net;

#endregion

namespace egrants_new.Integration.Models
{
    /// <summary>
    /// The web service history.
    /// </summary>
    public class WebServiceHistory
    {
        /// <summary>
        /// Gets or sets the ws history_ id.
        /// </summary>
        public int WSHistory_Id { get; set; }

        /// <summary>
        /// Gets or sets the web service.
        /// </summary>
        public WebServiceEndPoint WebService { get; set; }

        /// <summary>
        /// Gets or sets the web service name.
        /// </summary>
        public string WebServiceName { get; set; }

        /// <summary>
        /// Gets or sets the endpoint uri sent.
        /// </summary>
        public string EndpointUriSent { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Gets or sets the result status code.
        /// </summary>
        public HttpStatusCode ResultStatusCode { get; set; }

        /// <summary>
        /// Gets or sets the date triggered.
        /// </summary>
        public DateTimeOffset DateTriggered { get; set; }

        /// <summary>
        /// Gets or sets the date completed.
        /// </summary>
        public DateTimeOffset DateCompleted { get; set; }

        /// <summary>
        /// Gets or sets the exception message.
        /// </summary>
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// The update endpoint schedule.
        /// </summary>
        public void UpdateEndpointSchedule()
        {
            // Update the webservice endpoint record to reflect run date, time, and increment next run date
            this.WebService.MarkRunCompleted(this.ResultStatusCode);
        }
    }
}