using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static egrants_new.Integration.Models.IntegrationEnums;

namespace egrants_new.Integration.EmailRulesEngine.Models
{
    public class EmailMsgMetadata
    {
        public string Name { get; set; }
        public MetadataType type {get; set; }
        public Object metadata { get; set; }
    }
}