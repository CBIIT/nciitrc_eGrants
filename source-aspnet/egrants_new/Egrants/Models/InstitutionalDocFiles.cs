namespace egrants_new.Egrants.Models
{
        public class InstitutionalDocFiles
    {
            public int Tag { get; set; }
            public string org_id { get; set; }
            public string org_name { get; set; }
            public string document_id { get; set; }
            public string category_name { get; set; }
            public string url { get; set; }
            public string start_date { get; set; }
            public string end_date { get; set; }
            public string created_date { get; set; }
            public string comments { get; set; }
        }
}