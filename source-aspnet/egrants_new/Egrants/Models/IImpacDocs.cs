namespace egrants_new.Egrants.Models
{
    public interface IImpacDocs
    {
        string accepted_date { get; set; }
        string appl_id { get; set; }
        string category_name { get; set; }
        string created_date { get; set; }
        string full_grant_num { get; set; }
        string tag { get; set; }
        string url { get; set; }
    }
}