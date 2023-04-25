using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace egrants_new.Egrants.Models
{
    public class ApplModel
    {
        public int GrantId { get; set; }

        public int ApplId { get; set; }
        public string ApplTypeCode { get; set; }
        public string ActivityCode {get;set;}

        public int SupportYear { get; set; }
        public int? SuffixCode { get; set; }
        public string ProjectTitle { get; set; }
        public string OrgName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }


    }
}