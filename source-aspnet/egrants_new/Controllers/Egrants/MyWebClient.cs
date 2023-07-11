using System;
using System.Configuration;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace egrants_new.Controllers
{
    class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var cert_url = ConfigurationManager.ConnectionStrings["certPath"].ToString();
            var cert_pass = ConfigurationManager.ConnectionStrings["certPass"].ToString();
            var certificate = new X509Certificate2(cert_url, cert_pass);

            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            
            if (request != null)
            {
                request.ClientCertificates.Add(certificate);
            }
    
            return request;
        }
    }
}