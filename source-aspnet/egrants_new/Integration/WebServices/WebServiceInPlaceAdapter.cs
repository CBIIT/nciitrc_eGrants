using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using egrants_new.Integration.Models;
using static egrants_new.Integration.Models.IntegrationEnums;
using Hangfire.Annotations;

namespace egrants_new.Integration.WebServices
{
    public class WebServiceInPlaceAdapter
    {
        private IEgrantWebService _ws = null;
        private InPlaceWebServiceFactory factory;



        public WebServiceInPlaceAdapter()
        {
            factory = new InPlaceWebServiceFactory();
        }


        public bool SetupNewWebServiceCall(IntegrationEnums.AuthenticationType authenticationType)
        {
            //This will need to be updated later, but since there's only one type
            var ws = factory.Make(AuthenticationType.OAuth);



            return true;
        }


        public class InPlaceWebServiceFactory
        {
          //  private IEgrantWebService ws = null;
            public IEgrantWebService Make(AuthenticationType authenticationType)
            {
                var ep = new WebServiceEndPoint();

                //Create an endpoint with the critical information needed to invoke the 
                ep.WebRequestMethod = "";
                ep.EndpointUri = "";
                ep.Action = "";
                ep.AuthenticationType = authenticationType;
                ep.NodeMappings = new List<WSNodeMapping>();
                ep.Params = new List<WebServiceParam>();
                ep.LastRun = new DateTimeOffset();
                ep.QueryString = "";

                var ws = new MicrosoftGraphOAuthService(ep);

                return ws;
            }

        }

}
}
