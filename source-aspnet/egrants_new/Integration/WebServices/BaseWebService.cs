using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Configuration;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using egrants_new.Integration.Models;

namespace egrants_new.Integration.WebServices
{
    public class BaseWebService : IEgrantWebService
    {
        public BaseWebService(WebServiceEndPoint ws)
        {
            WebService = ws;
        }
        public WebServiceEndPoint WebService { get; set; }
        public List<string> Errors { get; set; }

        public WebServiceHistory GetData()
        {
            WebServiceHistory history = new WebServiceHistory
            {
                WebService = WebService,
                WebServiceName = WebService.Name,
                DateTriggered = DateTimeOffset.Now
            };
            try
            {
                var result = string.Empty;
                var uriString = string.Join("/", WebService.EndpointUri, WebService.Action);

                uriString = string.Join(@"?", uriString, PrepareQueryString());
                history.EndpointUriSent = uriString;

                uriString =
                    "https://graph.microsoft.com/v1.0/me/messages?$filter=receivedDateTime+ge+2021-10-22T14:41:09Z";

                var uri = new Uri(uriString);

                HttpWebRequest webServiceRequest = (HttpWebRequest)WebRequest.Create(uri);
                webServiceRequest.Method = WebService.WebRequestMethod;
                webServiceRequest.Accept = WebService.AcceptsHeader;
                webServiceRequest.AllowAutoRedirect = WebService.AllowRedirect;
                webServiceRequest.KeepAlive = WebService.KeepAlive;

                if (WebService.Timeout > 0)
                {
                    webServiceRequest.Timeout = WebService.Timeout;
                }

                AddAuthentication(ref webServiceRequest);

                var response = (HttpWebResponse)webServiceRequest.GetResponse();
                Stream stream = response.GetResponseStream();
                if (stream != null)
                {
                    result = new StreamReader(stream).ReadToEnd();
                }

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

            return history;
        }

        public virtual void AddAuthentication(ref HttpWebRequest webRequest)
        {
            throw new NotImplementedException();
        }

        private string PrepareQueryString()
        {
            string paramString = string.Empty;

            foreach (WebServiceParam param in WebService.Params)
            {
                paramString += $"&{param.Name}={EvaluateParamValue(param.Value)}";
            }

            if (paramString.StartsWith("&"))
            {
                paramString = paramString.Substring(1);
            }

            return paramString;
        }

        private string EvaluateParamValue(string value)
        {
            if (value.StartsWith("##"))
            {

                switch (value)
                {
                    case "##LastRun":
                        {
                            var time = WebService.LastRun.TimeOfDay.ToString();
                            time = time.Split('.')[0];
                            var lastRun =
                                $"{WebService.LastRun.LocalDateTime.ToShortDateString()}%20{time}";

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
                            value = value.Replace(" PM", "");
                            value = value.Replace(" AM", "");
                            break;
                        }
                    case "##MaxId":
                        //TODO: Implement later
                        //This would look up the last Id of a primary key and return that to be included in the query string dynamically
                        break;

                    default:
                        break;
                }

                return value;
            }

            if (value.Contains("##"))
            {
                if (value.Contains("##LastRun"))
                {
                    var time = WebService.LastRun.TimeOfDay.ToString();
                    time = time.Split('.')[0];
                    var lastRun =
                        $"{WebService.LastRun.LocalDateTime.ToShortDateString()}%20{time}";

                    value = value.Replace("##LastRun", lastRun);
                }

                if (value.Contains("##Now24Hr"))
                {
                    var time = DateTime.Now.TimeOfDay.ToString();
                    time = time.Split('.')[0];
                    var now24 = $"{DateTime.Now.ToShortDateString()}%20{time}";
                    value = value.Replace("##Now24Hr", now24);//  $"{DateTime.Now.ToShortDateString()}%20{time}";
                }

                if (value.Contains("##Now"))
                {
                    value = $"{DateTime.Now.ToShortDateString()}%20{DateTime.Now.ToString("T")}";
                    value = value.Replace(" PM", "");
                    value = value.Replace(" AM", "");

                }

                if (value.Contains("##MaxId"))
                {
                    //TODO: Implement later
                    //This would look up the last Id of a primary key and return that to be included in the query string dynamically

                }
                return value;
            }

            return value;
        }

    }
    
}




