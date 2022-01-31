using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;


namespace egrants_new.Integration.Identity
{
    public sealed class IdentityImplementation
    {
        private string OAuthToken = String.Empty;
        private DateTime expires;
        private const int tokenLife = 3599;

        private static readonly IdentityImplementation instance = new IdentityImplementation();
        static IdentityImplementation()
        {
        }
        private IdentityImplementation()
        {
        }
        public static IdentityImplementation Instance
        {
            get
            {
                return instance;
            }
        }

        public string GetAuthorizationMicrosoftOAuth()
        {
            if (DateTime.Now > expires | string.IsNullOrWhiteSpace(OAuthToken))
            {
                OAuthToken = GetAuthTokenII();
            }

            return OAuthToken;
        }

        private async Task<string> GetAuthToken()
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

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(baseUri);
                var result = await httpClient.PostAsync(reqUri, content);
                resultOut = await result.Content.ReadAsStringAsync();
            }

            expires = DateTime.Now.AddSeconds(tokenLife);
            var authResults = JObject.Parse(resultOut);
            var token = (string)authResults["access_token"];

            return token;
        }

        public string GetAuthTokenII()
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

            //var content = new FormUrlEncodedContent(new[]
            //{
            //    new KeyValuePair<string, string>("client_id", clientId),
            //    new KeyValuePair<string, string>("scope", apiScope ),
            //    new KeyValuePair<string, string>("client_secret", clientSecret),
            //    new KeyValuePair<string, string>("grant_type", "client_credentials")
            //});

            var postData = $"client_id={clientId}";
            postData += $"&scope={apiScope}";
            postData += $"&client_secret={clientSecret}";
            postData += $"&grant_type=client_credentials";
            var data = Encoding.ASCII.GetBytes(postData);

            string uri = String.Concat(baseUri, reqUri);

            HttpWebRequest req = (HttpWebRequest)WebRequest.CreateHttp(uri);

            //req.Accept = 
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;

            using (var stream = req.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)req.GetResponse();
            resultOut = new StreamReader(response.GetResponseStream()).ReadToEnd();

            var authResults = JObject.Parse(resultOut);
            var token = (string)authResults["access_token"];

            return token;
        }

    }
}
