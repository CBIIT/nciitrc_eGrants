using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace egrants_new.Egrants.Models
{
    /// <summary>
    /// DTO for the person coming back from the IRDB
    /// </summary>
    public class PersonContact
    {
        public string appl_id { get; set; }
        public long person_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string src_mi_name { get; set; }
        public string email_addr { get; set; }
    }
}