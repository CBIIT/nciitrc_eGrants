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
using Hangfire.Dashboard.Resources;
using Newtonsoft.Json.Linq;
using Microsoft.Exchange.WebServices;
using Microsoft.Exchange.WebServices.Auth;
using Microsoft.Exchange.WebServices.Data;

namespace egrants_new.Integration.WebServices
{
    public class EwsClient : BaseWebService
    {

        public EwsClient(WebServiceEndPoint ws) : base(ws)
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
                    ConnectExchange();



                    //history.ResultStatusCode = response.StatusCode;
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
                var clientAppId = ConfigurationManager.AppSettings["clientiduri"];
                IConfidentialClientApplication confidentialClientApp = ConfidentialClientApplicationBuilder.Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithAuthority(AadAuthorityAudience.AzureAdMyOrg, true)
                    .WithTenantId(tenantId)
                    .Build();
                var securestring = new System.Security.SecureString();//"XOdark9922144!@".ToCharArray(), 15);
                foreach (char c in "XOdark9922144!@".ToCharArray())
                {
                    securestring.AppendChar(c);
                }

                IEnumerable<string> scope = new string[] { string.Concat(clientAppId, "/.default") };

                var token = confidentialClientApp
                    .AcquireTokenForClient(scope)
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


        private void ConnectExchange()
        {

            var serverURI = new Uri("https://mail.nih.gov/ews/exchange.asmx");
            var exch = new Microsoft.Exchange.WebServices.Data.ExchangeService();
            exch.Url = serverURI;
            exch.UseDefaultCredentials = false;
            exch.Credentials = new System.Net.NetworkCredential("shellba", "XOdark9922144!@","NIH");
            // or to impersonate a network account use: 
            // var netCredential  =  Net.NetworkCredential = Net.CredentialCache.DefaultNetworkCredentials
            // exch.Credentials = netCredential
            var folderVw = new FolderView(999)
            {
                Traversal = FolderTraversal.Deep
            };
            //folderVw.Traversal = FolderTraversal.Deep;
            //var deletedItems = FindItemsResults(Of Item) = Nothing

            var info = exch.ServerInfo;

            var msg = new EmailMessage(exch)
            {
                From = "benny.shell@nih.gov",
                Subject = "Testing Email",
                Body = "This a testing message",
            };
            msg.ToRecipients.Add("benny.shell@nih.gov");

            msg.From = "benny.shell@nih.gov";
            msg.ToRecipients.Add("benny.shell@nih.gov");
            msg.Subject = "Testing Email";
            msg.Body = "This a testing message";
            msg.SendAndSaveCopy(WellKnownFolderName.SentItems);
            var inbox = exch.FindItems(WellKnownFolderName.Inbox, folderVw);
            foreach (var item in inbox)
            {
                var subject = item.Subject;
            }

         }




    }

}