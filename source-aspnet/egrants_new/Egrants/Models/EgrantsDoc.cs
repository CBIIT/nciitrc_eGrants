using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Xml.Linq;
using System.IO;
using System.Xml;

namespace egrants_new.Egrants.Models
{
    public class EgrantsDoc
    {
        //to save document error message that reported by user 
        public static void report_doc_error(string errormsg, int doc_id, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_doc_error", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@error", System.Data.SqlDbType.VarChar).Value = errormsg;
            cmd.Parameters.Add("@docid", System.Data.SqlDbType.Int).Value = doc_id;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            SqlDataReader rdr = cmd.ExecuteReader();

            conn.Close();
        }

        public class former_appls
        {
            public string former_num { get; set; }
            public string former_appl_id { get; set; }
        }

        //to load former appls
        public static List<former_appls> LoadFormerAppls(int grant_id)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT distinct former_num, former_appl_id FROM dbo.IMPP_Admin_Supplements_WIP "+
            " WHERE Serial_num =(select serial_num from grants where grant_id = @grant_id)", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@grant_id", System.Data.SqlDbType.Int).Value = grant_id;          
            conn.Open();

            var FormerAppls = new List<former_appls>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                FormerAppls.Add(new former_appls
                {
                    former_num= rdr["former_num"]?.ToString(),
                    former_appl_id=rdr["former_appl_id"]?.ToString()
                });
            }
            conn.Close();
            return FormerAppls;
        }

        public class supplement
        {
            public string tag { get; set; }
            public string id { get; set; }
            public string grant_id { get; set; }          
            public string serial_num { get; set; }           
            public string full_grant_num { get; set; }
            public string former_appl_id { get; set; }
            public string supp_appl_id { get; set; }
            public string support_year { get; set; }
            public string suffix_code { get; set; }
            public string former_num { get; set; }
            public string date_of_submitted { get; set; }
            public string category_name { get; set; }
            public string sub_category_name { get; set; }
            public string status { get; set; }
            public string url { get; set; }
            public string moved_date { get; set; }
            public string moved_by { get; set; }
            public string accession_number { get; set; }
            public string eRa_TS { get; set; }
        }

        //to load supplement documents
        public static List<supplement> LoadSupplement(string act, int grant_id, int support_year, string suffix_code, int former_applid, string docid_str, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_supplement", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@grant_id", System.Data.SqlDbType.Int).Value = grant_id;
            cmd.Parameters.Add("@support_year", System.Data.SqlDbType.Int).Value = support_year;
            cmd.Parameters.Add("@suffix_code", System.Data.SqlDbType.VarChar).Value = suffix_code;
            cmd.Parameters.Add("@former_applid", System.Data.SqlDbType.VarChar).Value = former_applid;
            cmd.Parameters.Add("@docid_str", System.Data.SqlDbType.VarChar).Value = docid_str;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@Operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            var Supplement = new List<supplement>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Supplement.Add(new supplement
                {
                    tag = rdr["tag"]?.ToString(),
                    grant_id = rdr["grant_id"]?.ToString(),
                    serial_num = rdr["serial_num"]?.ToString(),
                    id = rdr["id"]?.ToString(),
                    full_grant_num = rdr["full_grant_num"]?.ToString(),
                    supp_appl_id = rdr["supp_appl_id"]?.ToString(),
                    support_year = rdr["support_year"]?.ToString(),
                    suffix_code = rdr["suffix_code"]?.ToString(),
                    former_num = rdr["former_num"]?.ToString(),
                    former_appl_id = rdr["former_appl_id"]?.ToString(),
                    date_of_submitted = rdr["date_of_submitted"]?.ToString(),
                    category_name = rdr["category_name"]?.ToString(),
                    sub_category_name= rdr["sub_category_name"]?.ToString(),
                    status = rdr["status"]?.ToString(),
                    url = rdr["url"]?.ToString(),
                    moved_date = rdr["moved_date"]?.ToString(),
                    moved_by = rdr["moved_by"]?.ToString(),
                    accession_number = rdr["accession_number"]?.ToString(),
                    eRa_TS = rdr["eRa_TS"]?.ToString()
                });
            }
            conn.Close();
            return Supplement;
        }

        //to modify document index, delete, store or update for normal document 
        public static void doc_modify(string act, int appl_id, int category_id, string sub_category, string doc_date, string docid_str, string file_type, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_doc_modify", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@appl_id", System.Data.SqlDbType.Int).Value = appl_id;
            cmd.Parameters.Add("@category_id", System.Data.SqlDbType.Int).Value = category_id;
            cmd.Parameters.Add("@sub_category", System.Data.SqlDbType.VarChar).Value = sub_category;
            cmd.Parameters.Add("@doc_date", System.Data.SqlDbType.VarChar).Value = doc_date;
            cmd.Parameters.Add("@docid_str", System.Data.SqlDbType.VarChar).Value = docid_str;
            cmd.Parameters.Add("@file_type", System.Data.SqlDbType.VarChar).Value = file_type;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            SqlDataReader rdr = cmd.ExecuteReader();

            rdr.Close();
            conn.Close();
        }

        //to load specialist list
        public static List<EgrantsCommon.EgrantsUsers> LoadUsers(string ic)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select person_id, person_name from vw_people where position_id>1 and ic=@ic order by person_name", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;

            var UserList = new List<EgrantsCommon.EgrantsUsers>();
            conn.Open();

            cmd.CommandTimeout = 120;
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                UserList.Add(new EgrantsCommon.EgrantsUsers
                {
                    person_id = rdr["person_id"]?.ToString(),
                    person_name = rdr["person_name"]?.ToString()
                });
            }
            conn.Close();
            return UserList;

        }

        public class EgrantsCategories
        {
            public string category_id { get; set; }
            public string category_name { get; set; }
            public string package { get; set; }
            public string input_type { get; set; }
            public string input_constraint { get; set; }

        }
 

        //to load all Category list by ic
        ////public static List<EgrantsCategories> LoadCategoryList(string ic)
        ////{
        ////    System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
        ////    System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select category_id, category_name,package,input_type,input_constraint from vw_categories where ic=@ic order by category_name", conn);
        ////    cmd.CommandType = CommandType.Text;
        ////    cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;   //Session["ic"];

        ////    conn.Open();

        ////    var CategoryList = new List<EgrantsCategories>();
        ////    SqlDataReader rdr = cmd.ExecuteReader();
        ////    while (rdr.Read())
        ////    {

        ////        CategoryList.Add(new EgrantsCategories
        ////        {
        ////            category_id = rdr["category_id"]?.ToString(),
        ////            category_name = rdr["category_name"]?.ToString(),
        ////            package = rdr["package"]?.ToString(),
        ////            input_type = rdr["input_type"]?.ToString(),
        ////            input_constraint = rdr["input_constraint"]?.ToString(),
        ////        });
        ////    }
        ////    conn.Close();
        ////    return CategoryList;
        ////}

        //to load Category list that could be uploaded by ic and it is for create new only
        public static List<EgrantsCategories> LoadCategories(string ic)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select category_id, category_name,package,input_type,input_constraint from vw_categories where ic=@ic and can_upload='yes' order by category_name", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;   //Session["ic"];

            conn.Open();

            var CategoryList = new List<EgrantsCategories>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                CategoryList.Add(new EgrantsCategories
                {
                    category_id = rdr["category_id"]?.ToString(),
                    category_name = rdr["category_name"]?.ToString(),
                    package = rdr["package"]?.ToString(),
                    input_type = rdr["input_type"]?.ToString(),
                    input_constraint = rdr["input_constraint"]?.ToString(),
                });
            }
            conn.Close();
            return CategoryList;
        }


        //to load max Categoryid
        public static int GetMaxCategoryid(string ic)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select max(category_id) as max_category_id from vw_categories where ic=@ic", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;   
            conn.Open();
            int MaxCategoryid = 0;
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                MaxCategoryid = Convert.ToInt32(rdr["max_category_id"]);
            }
            conn.Close();
            return MaxCategoryid;
        }

        public class EgrantsSubCategories
        {
            public string parent_category_id { get; set; }
            public string sub_category_name { get; set; }

        }

        //to load sub category list
        public static List<EgrantsSubCategories> LoadSubCategoryList()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select parent_category_id,sub_category_name FROM categories_subcat_lookup", conn);
            cmd.CommandType = CommandType.Text;

            conn.Open();

            var SubCategoryList = new List<EgrantsSubCategories>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                SubCategoryList.Add(new EgrantsSubCategories
                {
                    parent_category_id = rdr["parent_category_id"]?.ToString(),
                    sub_category_name = rdr["sub_category_name"]?.ToString(),
                });
            }
            rdr.Close();
            conn.Close();
            return SubCategoryList;
        }

        public class DocumentInforamtion
        {
            public string admin_phs_org_code { get; set; }
            public string serial_num { get; set; }
            public string full_grant_num { get; set; }
            public string appl_id { get; set; }
            public string category_id { get; set; }
            public string sub_category_name { get; set; }
            public string document_id { get; set; }           
            public string document_date { get; set; }
            public string document_name { get; set; }
            
        }

        //to return document information 
        public static List<DocumentInforamtion> GetDocInfo(int doc_id)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            //System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select document_name + ' (' + convert(varchar, document_date, 101) +')' as DocumentInfo from egrants where document_id = @document_id", conn);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select admin_phs_org_code, serial_num, full_grant_num, appl_id, category_id, ISNULL(sub_category_name, null) as sub_category_name, document_id, document_name, convert(varchar, document_date, 101) as document_date from egrants where document_id = @document_id", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@document_id", System.Data.SqlDbType.Int).Value = doc_id;
            conn.Open();

            var DocInfo = new List<DocumentInforamtion>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                DocInfo.Add(new DocumentInforamtion
                {
                    admin_phs_org_code = rdr["admin_phs_org_code"].ToString(),
                    serial_num = rdr["serial_num"]?.ToString(),
                    appl_id = rdr["appl_id"]?.ToString(),
                    category_id = rdr["category_id"]?.ToString(),
                    sub_category_name = rdr["sub_category_name"]?.ToString(),
                    full_grant_num = rdr["full_grant_num"]?.ToString(),
                    document_id = rdr["document_id"]?.ToString(),
                    document_date = rdr["document_date"]?.ToString(),
                    document_name = rdr["document_name"]?.ToString()
                });
            }

            rdr.Close();
            conn.Close();

            return DocInfo;

            //var document_info = (string)cmd.ExecuteScalar();
            //conn.Close();  
            //return document_info;
        }
        //to create a new document and return new document_id
        public static string GetDocID(int applid, int categoryid, string subcategory, DateTime docdate, string filetype, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_doc_create", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ApplID", System.Data.SqlDbType.Int).Value = applid;
            cmd.Parameters.Add("@CategoryID", System.Data.SqlDbType.Int).Value = categoryid;
            cmd.Parameters.Add("@SubCategory", System.Data.SqlDbType.VarChar).Value = subcategory;
            cmd.Parameters.Add("@DocDate", System.Data.SqlDbType.DateTime).Value = docdate;
            cmd.Parameters.Add("@file_type", System.Data.SqlDbType.VarChar).Value = filetype;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@DocumentID", System.Data.SqlDbType.Int);
            cmd.Parameters["@DocumentID"].Direction = System.Data.ParameterDirection.Output;
            conn.Open();

            System.Data.SqlClient.SqlDataReader DataReader = cmd.ExecuteReader();

            DataReader.Close();
            conn.Close();

            var document_id = Convert.ToString(cmd.Parameters["@DocumentID"].Value);
            return document_id;
        }

        public class DocTransactionHistory
        {
            public string transaction_type { get; set; }
            public string document_id { get; set; }
            public string full_grant_num { get; set; }
            public string category_name { get; set; }
            public string person_name { get; set; }
            public string url { get; set; }
            public string transaction_date { get; set; }
        }

        //load doc Transaction History
        public static List<DocTransactionHistory> LoadDocTransactionHistory(string transaction_type, int person_id, string start_date, string end_date, string date_range, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_management_doc_transaction_report", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@transaction_type", System.Data.SqlDbType.VarChar).Value = transaction_type;
            cmd.Parameters.Add("@startdate", System.Data.SqlDbType.VarChar).Value = start_date;
            cmd.Parameters.Add("@enddate", System.Data.SqlDbType.VarChar).Value = end_date;
            cmd.Parameters.Add("@date_range", System.Data.SqlDbType.VarChar).Value = date_range;
            cmd.Parameters.Add("@person_id", System.Data.SqlDbType.Int).Value = person_id;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();
            var EgrantsDocs = new List<DocTransactionHistory>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                EgrantsDocs.Add(new DocTransactionHistory
                {
                    transaction_type = rdr["transaction_type"]?.ToString(),
                    document_id = rdr["document_id"]?.ToString(),
                    full_grant_num = rdr["full_grant_num"]?.ToString(),
                    category_name = rdr["category_name"]?.ToString(),
                    person_name = rdr["person_name"]?.ToString(),
                    url = rdr["url"]?.ToString(),
                    transaction_date = rdr["transaction_date"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();

            return EgrantsDocs;
        }


        public class ImpacDocs
        {
            public string tag { get; set; }
            public string appl_id { get; set; }
            public string full_grant_num { get; set; }
            public string accepted_date { get; set; }
            public string category_name { get; set; }
            public string created_date { get; set; }
            public string url { get; set; }
        }

        public static List<ImpacDocs> LoadImpacDocs(string act, int appl_id)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_impac_docs", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            //cmd.Parameters.Add("@image_server", System.Data.SqlDbType.VarChar).Value = image_server;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@appl_id", System.Data.SqlDbType.Int).Value = appl_id;
            conn.Open();

            var ImpactDocs = new List<ImpacDocs>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                ImpactDocs.Add(new ImpacDocs
                {
                    tag = rdr["tag"]?.ToString(),
                    appl_id = rdr["appl_id"]?.ToString(),
                    full_grant_num = rdr["full_grant_num"]?.ToString(),
                    accepted_date = rdr["accepted_date"]?.ToString(),
                    category_name = rdr["category_name"]?.ToString(),
                    created_date = rdr["created_date"]?.ToString(),
                    url = rdr["url"]?.ToString()
                });
            }
            conn.Close();
            return ImpactDocs;
        }

        public class DocsUnidentified
        {         
            public string document_id { get; set; }
            public string document_name { get; set; }
            public string document_date { get; set; }
            public string created_by { get; set; }
            public string created_date { get; set; }
            public string category_id { get; set; }
            public string qc_date { get; set; }
            public string url { get; set; }       
        }

        public static List<DocsUnidentified> LoadDocsUnidentified(string imageserver, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT document_id,convert(varchar, document_date, 101) as document_date, document_name," +
            " created_by,convert(varchar, created_date, 101) as created_date, convert(varchar, qc_date, 101) as qc_date, category_id, @imageserver + url as url from egrants" +
            " where appl_id is null and qc_date is not null and parent_id is null and qc_userid=@operator", conn);
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.Add("@imageserver", System.Data.SqlDbType.VarChar).Value = imageserver;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();
            var UnidentifiedDocs = new List<DocsUnidentified>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                UnidentifiedDocs.Add(new DocsUnidentified
                {                    
                    document_id = rdr["document_id"]?.ToString(),
                    document_name = rdr["document_name"]?.ToString(),
                    document_date = rdr["document_date"]?.ToString(),
                    category_id = rdr["category_id"].ToString(),
                    created_date = rdr["created_date"]?.ToString(),
                    created_by = rdr["created_by"]?.ToString(),
                    qc_date = rdr["qc_date"]?.ToString(),
                    url = rdr["url"]?.ToString()                    
                });
            }
            rdr.Close();
            conn.Close();

            return UnidentifiedDocs;
        }

        public class DocAttachment
        {         
            public string document_name { get; set; }
            public string url { get; set; }
          
        }

        //load doc attachments
        public static List<DocAttachment> LoadDocAttachments(int document_id)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select url, document_name FROM vw_attachments WHERE document_id=@document_id", conn);
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.Add("@document_id", System.Data.SqlDbType.Int).Value = document_id;

            conn.Open();
            var DocAttachments = new List<DocAttachment>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                DocAttachments.Add(new DocAttachment
                {                   
                    document_name = rdr["document_name"]?.ToString(),                 
                    url = rdr["url"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();

            return DocAttachments;
        }

        public class Notification
        {
            public string notificationName { get; set; }
            public string Id { get; set; }
            public string description { get; set; }
            public string sentDate { get; set; }
            public string fromAddress { get; set; }
            public string toAddress { get; set; }
            public string ccAddress { get; set; }
            public string subject { get; set; }
            public string emailContent { get; set; }    
        }
        
        public static Notification getCloseoutNotif(string applid, string notifName)
        {
            string cert_url = ConfigurationManager.ConnectionStrings["certPath"].ToString();
            string cert_pass = ConfigurationManager.ConnectionStrings["certPass"].ToString();
            X509Certificate2 certificate = new X509Certificate2(cert_url, cert_pass);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"https://s2s.era.nih.gov/grantfolder/services/GrantDocumentInfo");
            request.ContentType = "application/xml";
            request.Method = "POST";
            request.ClientCertificates.Add(certificate);

            XmlDocument SOAPReqBody = new XmlDocument();
            SOAPReqBody.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>  
                    <soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""
                    xmlns:mes=""http://era.nih.gov/grantDocumentInfo/message""> 
                    <soap:Header/> 
                    <soap:Body>
                    <mes:GrantCorrespondenceRequest>
                    <mes:applId>" + applid + @"</mes:applId>               
                    </mes:GrantCorrespondenceRequest> 
                    </soap:Body>
                    </soap:Envelope>");

            using (Stream stream = request.GetRequestStream())
            {
                SOAPReqBody.Save(stream);
            }
            using (WebResponse Serviceres = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(Serviceres.GetResponseStream()))
                {
                    string ServiceResult = rd.ReadToEnd();
                    int pos = ServiceResult.IndexOf("apache.org>") + "apache.org>".Length;
                    ServiceResult = ServiceResult.Substring(pos);
                    pos = ServiceResult.IndexOf("--uuid:");
                    ServiceResult = ServiceResult.Substring(0, pos);
                    XDocument doc = XDocument.Parse(ServiceResult);
                    //return doc;
                    XNamespace ns2 = "http://era.nih.gov/grantDocumentInfo/domain";
                    IEnumerable<XElement> responses = doc.Descendants(ns2 + "correspondenceData");
                    string notif_name = null;
                    Notification notif = new Notification();
                    foreach (XElement response in responses)
                    {
                        notif_name = (string)response.Element(ns2 + "notificationName");
                        if (notif_name.ToLower() == notifName.ToLower())
                        {
                            notif.notificationName = notif_name;
                            notif.description = (string)response.Element(ns2 + "description");
                            notif.sentDate = (string)response.Element(ns2 + "sentDate");
                            notif.fromAddress = (string)response.Element(ns2 + "fromAddress");
                            notif.toAddress = (string)response.Element(ns2 + "toAddress");
                            notif.ccAddress = (string)response.Element(ns2 + "ccAddress");
                            notif.subject = (string)response.Element(ns2 + "subject");
                            string mailbody = (string)response.Element(ns2 + "emailContent");
                            //mailbody = mailbody.Replace("<pre>", "");
                            //mailbody = mailbody.Replace("</pre>", "");
                            notif.emailContent = mailbody;                           
                        }
                    }
                    return notif;
                }
            }
        }
    }
}