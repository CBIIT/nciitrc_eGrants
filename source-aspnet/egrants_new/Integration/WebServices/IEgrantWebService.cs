using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using egrants_new.Integration.Models;

namespace egrants_new.Integration.WebServices
{
    public interface IEgrantWebService
    {
        List<String> Errors { get; set; }
        WebServiceEndPoint WebService { get; set; }
        WebServiceHistory GetData();
        void AddAuthentication(ref HttpWebRequest webRequest);
    }
}
