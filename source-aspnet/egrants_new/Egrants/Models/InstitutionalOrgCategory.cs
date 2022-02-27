using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace egrants_new.Egrants.Models
{
    public class InstitutionalOrgCategory
    {
        public string category_id { get; set; }
        public string category_name { get; set; }
        public string tobe_flag { get; set; }
        public string flag_period { get; set; }
        public string flag_data { get; set; }
        public string today { get; set; }
    }
}