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
using System.Security.Cryptography.X509Certificates;
using System.Web;
using egrants_new.Integration.Models;

namespace egrants_new.Integration.WebServices
{
    public class MicrosoftGraphOAuthService : BaseWebService
    {

        public MicrosoftGraphOAuthService(WebServiceEndPoint ws) : base(ws)
        {
            base.WebService.AuthenticationType = Enumerations.AuthenticationType.OAuth;
        }

        public override void AddAuthentication(ref HttpWebRequest webRequest)
        {
            if (WebService.AuthenticationType == Enumerations.AuthenticationType.OAuth)
            {
                string oAuthToken = ConfigurationManager.AppSettings["MicrosoftGraphToken"];

                if (!string.IsNullOrWhiteSpace(oAuthToken))
                {
                    try
                    {

                        webRequest.PreAuthenticate = true;
                        webRequest.Headers.Add("Authorization", "Bearer " + oAuthToken);
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