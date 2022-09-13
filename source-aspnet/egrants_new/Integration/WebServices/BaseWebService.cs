#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  BaseWebService.cs
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
using System.IO;
using System.Net;

using egrants_new.Integration.Models;

#endregion

namespace egrants_new.Integration.WebServices
{
    /// <summary>
    /// The base web service.
    /// </summary>
    public class BaseWebService : IEgrantWebService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseWebService"/> class.
        /// </summary>
        /// <param name="ws">
        /// The ws.
        /// </param>
        public BaseWebService(WebServiceEndPoint ws)
        {
            this.WebService = ws;
        }

        /// <summary>
        /// Gets or sets the web service.
        /// </summary>
        public WebServiceEndPoint WebService { get; set; }

        /// <summary>
        /// Gets or sets the errors.
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// The get data.
        /// </summary>
        /// <returns>
        /// The <see cref="WebServiceHistory"/>.
        /// </returns>
        public WebServiceHistory GetData()
        {
            var history = new WebServiceHistory();
            history.WebService = this.WebService;
            history.WebServiceName = this.WebService.Name;
            history.DateTriggered = DateTimeOffset.Now;

            try
            {
                var result = string.Empty;
                var uristring = string.Join("/", this.WebService.EndpointUri, this.WebService.Action);

                uristring = string.Join(@"?", uristring, this.PrepareQueryString());
                history.EndpointUriSent = uristring;

                var uri = new Uri(uristring);

                var webServiceRequest = (HttpWebRequest)WebRequest.Create(uri);
                webServiceRequest.Method = this.WebService.WebRequestMethod;
                webServiceRequest.Accept = this.WebService.AcceptsHeader;
                webServiceRequest.AllowAutoRedirect = this.WebService.AllowRedirect;
                webServiceRequest.KeepAlive = this.WebService.KeepAlive;

                if (this.WebService.Timeout > 0)
                    webServiceRequest.Timeout = this.WebService.Timeout;

                this.AddAuthentication(ref webServiceRequest);

                var response = (HttpWebResponse)webServiceRequest.GetResponse();
                var stream = response.GetResponseStream();

                if (stream != null)
                    result = new StreamReader(stream).ReadToEnd();

                history.ResultStatusCode = response.StatusCode;
                history.Result = result;
            }
            catch (WebException ex)
            {
                var failedResponse = (HttpWebResponse)ex.Response;
                history.ResultStatusCode = failedResponse.StatusCode;
                history.ExceptionMessage = $"WebService {this.WebService.Name} encountered an exception: {ex.Message} {ex.StackTrace}";
            }

            history.DateCompleted = DateTimeOffset.Now;
            history.UpdateEndpointSchedule();

            return history;
        }

        /// <summary>
        /// The add authentication.
        /// </summary>
        /// <param name="webRequest">
        /// The web request.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public virtual void AddAuthentication(ref HttpWebRequest webRequest)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The prepare query string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string PrepareQueryString()
        {
            var paramString = string.Empty;

            foreach (var param in this.WebService.Params)
                paramString += $"&{param.Name}={this.EvaluateParamValue(param.Value)}";

            if (paramString.StartsWith("&"))
                paramString = paramString.Substring(1);

            return paramString;
        }

        /// <summary>
        /// The evaluate param value.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string EvaluateParamValue(string value)
        {
            if (value.StartsWith("##"))
                switch (value)
                {
                    case "##LastRun":
                        {
                            var time = this.WebService.LastRun.TimeOfDay.ToString();
                            time = time.Split('.')[0];
                            var lastRun = $"{this.WebService.LastRun.LocalDateTime.ToShortDateString()}%20{time}";

                            value = lastRun;
                        }

                        break;
                    case "##Now24Hr":
                        {
                            var time = DateTime.Now.TimeOfDay.ToString();
                            time = time.Split('.')[0];
                            value = $"{DateTime.Now.ToShortDateString()}%20{time}";
                        }

                        break;
                    case "##Now":
                        {
                            value = $"{DateTime.Now.ToShortDateString()}%20{DateTime.Now.ToString("T")}";
                            value = value.Replace(" PM", string.Empty);
                            value = value.Replace(" AM", string.Empty);

                            break;
                        }

                    case "##MaxId":
                        // TODO: Implement later
                        // This would look up the last Id of a primary key and return that to be included in the query string dynamically
                        break;
                }

            return value;
        }
    }

}