using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using egrants_new.Integration.Models;
using static egrants_new.Integration.Models.IntegrationEnums;

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
        public IntegrationEnums.Interval Interval { get; set; }
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
        public IntegrationEnums.ReconciliationBehavior ReconciliationBehavior { get; set; }
        public string CertificatePath { get; set; }
        public string CertificatePwd { get; set; }
        public string IntervalTimeSpan { get; set; }

        public SaveType SaveType
        {
            get
            {
                return AuthenticationType == AuthenticationType.OAuth
                    ? SaveType.MicrosoftGraphApi
                    : SaveType.eRaWebServiceData;
            }
        }

        public WebServiceEndPoint()
        {
            NodeMappings = new List<WSNodeMapping>();
            Params = new List<WebServiceParam>();
        }
        /// <summary>
        /// This Method with increment the run date and time on the Endpoint
        /// </summary>
        public void MarkRunCompleted(HttpStatusCode status)
        {
            //Update the webservice endpoint record to reflect run date, time, and increment next run date
            //Used switch here in case we need to add logic in the future
            switch (status)
            {
                case HttpStatusCode.OK:
                    LastRun = DateTimeOffset.Now;
                    int iLoop = 0;
                    while (NextRun < LastRun)
                    {
                        NextRun = AddInterval(NextRun, IntervalTimeSpan);
                        iLoop++;
                        if (iLoop > 1000)
                        {
                            break;
                            //throw new Exception("Problem Setting next scheduled date, have administrator set in the database");
                        }
                    }
                    break;

                default:
                    break;
            }

        }

        private DateTimeOffset AddInterval(DateTimeOffset time, string interval)
        {
            string[] intervalParts = interval.Split(',');
            time = time.AddMinutes(int.Parse(intervalParts[0]));
            time = time.AddHours(int.Parse(intervalParts[1]));
            time = time.AddDays(int.Parse(intervalParts[2]));
            time = time.AddMonths(int.Parse(intervalParts[3]));
            time = time.AddYears(int.Parse(intervalParts[4]));

            return time;
        }
    }
}