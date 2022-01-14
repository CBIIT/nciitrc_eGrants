using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Identity.Client;
using egrants_new.Integration.Models;
using Hangfire.Dashboard.Resources;
using Newtonsoft.Json.Linq;
using System.Security;
using System.Text;

namespace egrants_new.Integration.WebServices
{
    public class MicrosoftGraphOAuthService : BaseWebService
    {

        public MicrosoftGraphOAuthService(WebServiceEndPoint ws) : base(ws)
        {
            base.WebService.AuthenticationType = IntegrationEnums.AuthenticationType.OAuth;
        }


        public override List<WebServiceHistory> GetData()
        {
            var output = new List<WebServiceHistory>();
            var result = string.Empty;
            var uriString = string.Join("/", WebService.EndpointUri, WebService.Action);
            var nextUri = string.Join(@"?", uriString, PrepareQueryString());

            while (!string.IsNullOrWhiteSpace(nextUri))
            {
                WebServiceHistory history = new WebServiceHistory
                {
                    WebService = WebService,
                    WebServiceName = WebService.Name,
                    DateTriggered = DateTimeOffset.Now,
                    EndpointUriSent = nextUri
                };

                try
                {
                    HttpWebRequest webServiceRequest = (HttpWebRequest)WebRequest.Create(nextUri);
                    webServiceRequest.Method = WebService.WebRequestMethod;
                    webServiceRequest.Accept = WebService.AcceptsHeader;
                    webServiceRequest.AllowAutoRedirect = WebService.AllowRedirect;
                    webServiceRequest.KeepAlive = WebService.KeepAlive;

                    if (WebService.Timeout > 0)
                    {
                        webServiceRequest.Timeout = WebService.Timeout;
                    }

                    AddAuthentication(ref webServiceRequest);
                    nextUri = ""; //Clear out in case of exception,  it will cause an infinite loop

                    var response = (HttpWebResponse)webServiceRequest.GetResponse();
                    Stream stream = response.GetResponseStream();
                    if (stream != null)
                    {
                        result = new StreamReader(stream).ReadToEnd();
                    }

                    //Check for More
                    var graphResults = JObject.Parse(result);
                    nextUri = (string)graphResults["@odata.nextLink"];

                    history.ResultStatusCode = response.StatusCode;
                    history.Result = result;
                }
                catch (WebException ex)
                {
                    HttpWebResponse failedResponse = (HttpWebResponse)ex.Response;
                    history.ResultStatusCode = failedResponse.StatusCode;
                    history.ExceptionMessage =
                        $"WebService {WebService.Name} encountered an exception: {ex.Message} {ex.StackTrace}";

                }

                history.DateCompleted = DateTimeOffset.Now;
                history.UpdateEndpointSchedule();
                output.Add(history);
            }

            return output;
        }


        public override void AddAuthentication(ref HttpWebRequest webRequest)
        {
            var tokenValue = GetAuthToken().Result;

            if (tokenValue != null)
            {
                webRequest.PreAuthenticate = true;
                webRequest.Headers.Add("Authorization", "Bearer " + tokenValue);
//                webRequest.Accept = "application/json";
            }

            else
            {
                throw new Exception("Something went wrong.  Authentication not set.");
            }
        }


        public async Task<string> GetAuthToken()
        {
            string resultOut = string.Empty;
            var clientId = ConfigurationManager.AppSettings["MSGraphClientId"];
            var tenantId = ConfigurationManager.AppSettings["MSGraphTenantId"];
            var clientSecret = ConfigurationManager.AppSettings["MSGraphSecret"];
            //var clientAppId = ConfigurationManager.AppSettings["MSGraphClientIdUri"];
            //var apiUserId = ConfigurationManager.AppSettings["MSGraphUserId"];
            //var apiUserPwd = ConfigurationManager.AppSettings["MSGraphUserPwd"];
            var apiScope = ConfigurationManager.AppSettings["MSGraphScopes"];

            var baseUri = "https://login.microsoftonline.com";
            var reqUri = $"/{tenantId}/oauth2/v2.0/token";

            var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("scope", apiScope ),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });

            //            byte[] byteArray = Encoding.UTF8.GetBytes(content.ToString());

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(baseUri);
                var result = await httpClient.PostAsync(reqUri, content);
                resultOut = await result.Content.ReadAsStringAsync();
            }

            var authResults = JObject.Parse(resultOut);
            var token = (string)authResults["access_token"];

            return token;
        }

    }

}