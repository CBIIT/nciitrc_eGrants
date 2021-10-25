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
    public class CertAuthWebService : BaseWebService
    {

        public CertAuthWebService(WebServiceEndPoint ws):base( ws)
        {
            base.WebService.AuthenticationType = IntegrationEnums.AuthenticationType.Certificate;
        }

        public override void AddAuthentication(ref HttpWebRequest webRequest)
        {
            if (WebService.AuthenticationType == IntegrationEnums.AuthenticationType.Certificate)
            {
                string certPath = ConfigurationManager.AppSettings[WebService.CertificatePath];
                string certPwd = ConfigurationManager.AppSettings[WebService.CertificatePwd];
                if (File.Exists(certPath))
                {
                    try
                    {
                        X509Certificate2 certificate = new X509Certificate2(certPath, certPwd);
                        webRequest.ClientCertificates.Add(certificate);
                    } 
                    catch
                    {
                        throw new Exception("Adding Certificate Failed");
                    }

                }
                else
                {
                    throw new FileNotFoundException("The Specified Client Certificate for Authentication could not be found");
                }
            }
            else
            {
                throw new Exception("Something when wrong.  Authentication not set.");
            }
        }
    }

}


