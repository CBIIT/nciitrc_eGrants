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
using System.Web;
using Microsoft.Identity.Client;
using egrants_new.Integration.Models;
using Newtonsoft.Json.Linq;

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


         public void AddAuthentication(ref HttpWebRequest webRequest)
        {
            if (WebService.AuthenticationType == IntegrationEnums.AuthenticationType.OAuth)
            {
                var clientId = ConfigurationManager.AppSettings["MSGraphClientId"];
                var tenantId = ConfigurationManager.AppSettings["MSGraphTenantId"];
                var clientSecret = ConfigurationManager.AppSettings["MSGraphSecret"];
                IConfidentialClientApplication confidentialClientApp =  ConfidentialClientApplicationBuilder.Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithAuthority(AadAuthorityAudience.AzureAdMyOrg, true)
                    .WithTenantId(tenantId)
                    .Build();
                string[] scopes = {"Mail.ReadWrite", "Mail.Send", "User.Read"};
                var securestring = new System.Security.SecureString();//"XOdark9922144!@".ToCharArray(), 15);
                foreach (char c in "XOdark9922144!@".ToCharArray())
                {
                    securestring.AppendChar(c);
                }

                var token = confidentialClientApp
                    .AcquireTokenForClient(scopes)
                    .WithForceRefresh(true)
                    .ExecuteAsync();

                //var oAuthTokenBuilder = publicClientApp.AcquireTokenSilent(scopes, accounts.FirstOrDefault());

                var oAuthToken = token.Result;

                //token.


                //string oAuthToken = ConfigurationManager.AppSettings["MicrosoftGraphToken"];
                //string oAuthToken = ConfigurationManager.AppSettings[WebService.CertificatePath];

                if (!string.IsNullOrWhiteSpace(oAuthToken.AccessToken))
                {
                    try
                    {

                        webRequest.PreAuthenticate = true;
                        webRequest.Headers.Add("Authorization", "Bearer " + token);
                        webRequest.Accept = "application/json";
                    }
                    catch
                    {
                        throw new Exception("Token Authorization Failed");
                    }
                }
                else
                {
                    throw new Exception("The authentication token was not found");
                }
            }
            else
            {
                throw new Exception("Something went wrong.  Authentication not set.");
            }
        }
    }

}