using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using egrants_new.Integration.Models;
using static egrants_new.Integration.Models.Enumerations;

namespace egrants_new.Integration.Models
{
    public class WebServiceEndPoint
    {
        public int WSEndpoint_Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string EndpointUri { get; set; }
        public string Action { get; set; }
        public string AcceptsHeader { get; set; }
        public List<WebServiceParam> Params { get; set; } 
        public AuthenticationType AuthenticationType { get; set; }
        public string SourceOrganization { get; set; }
        public DateTimeOffset NextRun { get; set; }
        public DateTimeOffset LastRun { get; set; }
        public DateTimeOffset LastTrigger { get; set; }
        public string DestinationDatabase { get; set; }
        public string DestinationTable { get; set; }
        public int Interval { get; set; }
        public string QueryString { get; set; }
        public DateTimeUnits Frequency { get; set; }
        public bool Enabled { get; set; }
        public bool RetryOnFail { get; set; }
        public int RetryInterval { get; set; }
        public DateTimeUnits RetryFreq { get; set; }
        public string WebRequestMethod { get; set; }
        public bool KeepAlive { get; set; }
        public int Timeout { get; set; }
        public bool AllowRedirect { get; set; }
        public string Database { get; set; }
        public string Schema { get; set; }
        public List<WSNodeMapping> NodeMappings { get; set; }
        public Enumerations.ReconciliationBehavior ReconciliationBehavior { get; set; }
        public string CertificatePath { get; set; }
        public string CertificatePwd { get; set; }
        public string IntervalTimeSpan { get; set; }

        public WebServiceEndPoint()
        {
            NodeMappings = new List<WSNodeMapping>();
            Params = new List<WebServiceParam>();
        }
    }
}