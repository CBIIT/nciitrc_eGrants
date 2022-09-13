#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  WebServiceEndPoint.cs
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
using System.Collections.Generic;
using System.Net;

using static egrants_new.Integration.Models.Enumerations;

#endregion

namespace egrants_new.Integration.Models
{
    /// <summary>
    /// The web service end point.
    /// </summary>
    public class WebServiceEndPoint
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServiceEndPoint"/> class.
        /// </summary>
        public WebServiceEndPoint()
        {
            this.NodeMappings = new List<WSNodeMapping>();
            this.Params = new List<WebServiceParam>();
        }

        /// <summary>
        /// Gets or sets the ws endpoint_ id.
        /// </summary>
        public int WSEndpoint_Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the endpoint uri.
        /// </summary>
        public string EndpointUri { get; set; }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the accepts header.
        /// </summary>
        public string AcceptsHeader { get; set; }

        /// <summary>
        /// Gets or sets the params.
        /// </summary>
        public List<WebServiceParam> Params { get; set; }

        /// <summary>
        /// Gets or sets the authentication type.
        /// </summary>
        public AuthenticationType AuthenticationType { get; set; }

        /// <summary>
        /// Gets or sets the source organization.
        /// </summary>
        public string SourceOrganization { get; set; }

        /// <summary>
        /// Gets or sets the next run.
        /// </summary>
        public DateTimeOffset NextRun { get; set; }

        /// <summary>
        /// Gets or sets the last run.
        /// </summary>
        public DateTimeOffset LastRun { get; set; }

        /// <summary>
        /// Gets or sets the last trigger.
        /// </summary>
        public DateTimeOffset LastTrigger { get; set; }

        /// <summary>
        /// Gets or sets the destination database.
        /// </summary>
        public string DestinationDatabase { get; set; }

        /// <summary>
        /// Gets or sets the destination table.
        /// </summary>
        public string DestinationTable { get; set; }

        /// <summary>
        /// Gets or sets the interval.
        /// </summary>
        public Interval Interval { get; set; }

        /// <summary>
        /// Gets or sets the query string.
        /// </summary>
        public string QueryString { get; set; }

        /// <summary>
        /// Gets or sets the frequency.
        /// </summary>
        public DateTimeUnits Frequency { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether retry on fail.
        /// </summary>
        public bool RetryOnFail { get; set; }

        /// <summary>
        /// Gets or sets the retry interval.
        /// </summary>
        public int RetryInterval { get; set; }

        /// <summary>
        /// Gets or sets the retry freq.
        /// </summary>
        public DateTimeUnits RetryFreq { get; set; }

        /// <summary>
        /// Gets or sets the web request method.
        /// </summary>
        public string WebRequestMethod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether keep alive.
        /// </summary>
        public bool KeepAlive { get; set; }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether allow redirect.
        /// </summary>
        public bool AllowRedirect { get; set; }

        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Gets or sets the schema.
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Gets or sets the node mappings.
        /// </summary>
        public List<WSNodeMapping> NodeMappings { get; set; }

        /// <summary>
        /// Gets or sets the reconciliation behavior.
        /// </summary>
        public ReconciliationBehavior ReconciliationBehavior { get; set; }

        /// <summary>
        /// Gets or sets the certificate path.
        /// </summary>
        public string CertificatePath { get; set; }

        /// <summary>
        /// Gets or sets the certificate pwd.
        /// </summary>
        public string CertificatePwd { get; set; }

        /// <summary>
        /// Gets or sets the interval time span.
        /// </summary>
        public string IntervalTimeSpan { get; set; }

        /// <summary>
        /// This Method with increment the run date and time on the Endpoint
        /// </summary>
        /// <param name="status">
        /// The status.
        /// </param>
        public void MarkRunCompleted(HttpStatusCode status)
        {
            // Update the webservice endpoint record to reflect run date, time, and increment next run date
            // Used switch here in case we need to add logic in the future
            switch (status)
            {
                case HttpStatusCode.OK:
                    this.LastRun = DateTimeOffset.Now;
                    var iLoop = 0;

                    while (this.NextRun < this.LastRun)
                    {
                        this.NextRun = this.AddInterval(this.NextRun, this.IntervalTimeSpan);
                        iLoop++;

                        if (iLoop > 1000)
                            break;

                        // throw new Exception("Problem Setting next scheduled date, have administrator set in the database");
                    }

                    break;
            }
        }

        /// <summary>
        /// The add interval.
        /// </summary>
        /// <param name="time">
        /// The time.
        /// </param>
        /// <param name="interval">
        /// The interval.
        /// </param>
        /// <returns>
        /// The <see cref="DateTimeOffset"/>.
        /// </returns>
        private DateTimeOffset AddInterval(DateTimeOffset time, string interval)
        {
            var intervalParts = interval.Split(',');
            time = time.AddMinutes(int.Parse(intervalParts[0]));
            time = time.AddHours(int.Parse(intervalParts[1]));
            time = time.AddDays(int.Parse(intervalParts[2]));
            time = time.AddMonths(int.Parse(intervalParts[3]));
            time = time.AddYears(int.Parse(intervalParts[4]));

            return time;
        }
    }
}