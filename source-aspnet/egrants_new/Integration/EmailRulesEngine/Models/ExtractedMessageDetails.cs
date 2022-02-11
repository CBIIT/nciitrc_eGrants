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
        public string StatusOfProcessing { get; set; }
        public string Filenumbername { get; set; }

        public string linkToDoc
        {
            get
            {
                if (Parentapplid > 0)
                {
                    return $"https://egrants-web-dev.nci.nih.gov/Egrants/by_appl?appl_id={Parentapplid}&mode=";
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}