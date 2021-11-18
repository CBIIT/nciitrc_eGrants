using System;

namespace egrants_new.Integration.EmailRulesEngine.Models

{
    public class ExtractedMessageDetails
    {
        public int Parentapplid { get; set; }
        public string Pa { get; set; }
        public DateTime Rcvd_dt { get; set; }
        public  string Catname { get; set; }
        public  string Filetype { get; set; }
        public string Subcatname { get; set; }
        public string Sub { get; set; }
        public string Body { get; set; } 

    }
}