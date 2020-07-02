using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace egrants_new.Integration.Models
{
    public class WebServiceHistory
    {
        public int WSHistory_Id { get; set; }
        public WebServiceEndPoint WebService { get; set; }
        public string WebServiceName { get; set;}
        public string EndpointUriSent { get; set; }
        public string Result { get; set; }
        public HttpStatusCode ResultStatusCode { get; set; }
        public DateTimeOffset DateTriggered { get; set; }
        public DateTimeOffset DateCompleted { get; set; }
        public string ExceptionMessage { get; set; }
    }
}