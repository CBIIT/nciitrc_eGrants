using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Linq;

using egrants_new.Models;

namespace egrants_new.Egrants.Models
{
    /// <summary>
    ///     The egrants doc.
    /// </summary>
    public class EgrantsDoc
    {
        // to save document error message that reported by user 
        /// <summary>
        ///     The report_doc_error.
        /// </summary>
        /// <param name="errormsg">The errormsg.</param>
        /// <param name="doc_id">The doc_id.</param>
        /// <param name="ic">The ic.</param>
        /// <param name="userid">The userid.</param>
        public static void report_doc_error(string errormsg, int doc_id, string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_doc_error", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@error", SqlDbType.VarChar).Value = errormsg;
            cmd.Parameters.Add("@docid", SqlDbType.Int).Value = doc_id;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
            conn.Open();

            var rdr = cmd.ExecuteReader();

            conn.Close();
        }

        // to load former appls
        /// <summary>
        ///     The load former appls.
        /// </summary>
        /// <param name="grant_id">The grant_id.</param>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public static List<former_appls> LoadFormerAppls(int grant_id)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "SELECT distinct former_num, former_appl_id FROM dbo.IMPP_Admin_Supplements_WIP "
              + " WHERE Serial_num =(select serial_num from grants where grant_id = @grant_id)",
                conn
            );

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@grant_id", SqlDbType.Int).Value = grant_id;
            conn.Open();

            var FormerAppls = new List<former_appls>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                FormerAppls.Add(new former_appls { former_num = rdr["former_num"]?.ToString(), former_appl_id = rdr["former_appl_id"]?.ToString() });

            conn.Close();

            return FormerAppls;
        }

        // to load supplement documents
        /// <summary>
        ///     The load supplement.
        /// </summary>
        /// <param name="act">The act.</param>
        /// <param name="grant_id">The grant_id.</param>
        /// <param name="support_year">The support_year.</param>
        /// <param name="suffix_code">The suffix_code.</param>
        /// <param name="former_applid">The former_applid.</param>
        /// <param name="docid_str">The docid_str.</param>
        /// <param name="ic">The ic.</param>
        /// <param name="userid">The userid.</param>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public static List<supplement> LoadSupplement(
            string act,
            int grant_id,
            int support_year,
            string suffix_code,
            int former_applid,
            string docid_str,
            string ic,
            string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_supplement", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@grant_id", SqlDbType.Int).Value = grant_id;
            cmd.Parameters.Add("@support_year", SqlDbType.Int).Value = support_year;
            cmd.Parameters.Add("@suffix_code", SqlDbType.VarChar).Value = suffix_code;
            cmd.Parameters.Add("@former_applid", SqlDbType.VarChar).Value = former_applid;
            cmd.Parameters.Add("@docid_str", SqlDbType.VarChar).Value = docid_str;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@Operator", SqlDbType.VarChar).Value = userid;
            conn.Open();

            var Supplement = new List<supplement>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                Supplement.Add(
                    new supplement
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
                            date_of_submitted = rdr["submitted_date"]?.ToString(),
                            category_name = rdr["category_name"]?.ToString(),
                            sub_category_name = rdr["sub_category_name"]?.ToString(),
                            status = rdr["status"]?.ToString(),
                            url = rdr["url"]?.ToString(),
                            moved_date = rdr["moved_date"]?.ToString(),
                            moved_by = rdr["moved_by"]?.ToString(),
                            accession_number = rdr["accession_number"]?.ToString()
                        }
                );

            conn.Close();

            return Supplement;
        }

        // to modify document index, delete, store or update for normal document 
        /// <summary>
        ///     The doc_modify.
        /// </summary>
        /// <param name="act">The act.</param>
        /// <param name="appl_id">The appl_id.</param>
        /// <param name="category_id">The category_id.</param>
        /// <param name="sub_category">The sub_category.</param>
        /// <param name="doc_date">The doc_date.</param>
        /// <param name="docid_str">The docid_str.</param>
        /// <param name="file_type">The file_type.</param>
        /// <param name="ic">The ic.</param>
        /// <param name="userid">The userid.</param>
        public static void doc_modify(
            string act,
            int appl_id,
            int category_id,
            string sub_category,
            string doc_date,
            string docid_str,
            string file_type,
            string ic,
            string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_doc_modify", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@appl_id", SqlDbType.Int).Value = appl_id;
            cmd.Parameters.Add("@category_id", SqlDbType.Int).Value = category_id;
            cmd.Parameters.Add("@sub_category", SqlDbType.VarChar).Value = sub_category;
            cmd.Parameters.Add("@doc_date", SqlDbType.VarChar).Value = doc_date;
            cmd.Parameters.Add("@docid_str", SqlDbType.VarChar).Value = docid_str;
            cmd.Parameters.Add("@file_type", SqlDbType.VarChar).Value = file_type;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
            conn.Open();

            var rdr = cmd.ExecuteReader();

            rdr.Close();
            conn.Close();
        }

        // to load specialist list
        /// <summary>
        ///     The load users.
        /// </summary>
        /// <param name="ic">The ic.</param>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public static List<EgrantsCommon.EgrantsUsers> LoadUsers(string ic)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            var cmd = new SqlCommand("select person_id, person_name from vw_people where position_id>1 and ic=@ic order by person_name", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;

            var UserList = new List<EgrantsCommon.EgrantsUsers>();
            conn.Open();

            cmd.CommandTimeout = 120;
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                UserList.Add(
                    new EgrantsCommon.EgrantsUsers { person_id = rdr["person_id"]?.ToString(), person_name = rdr["person_name"]?.ToString() });

            conn.Close();

            return UserList;
        }

        // to load all Category list by ic
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

        // to load Category list that could be uploaded by ic and it is for create new only
        /// <summary>
        ///     The load categories.
        /// </summary>
        /// <param name="ic">The ic.</param>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public static List<EgrantsCategories> LoadCategories(string ic)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "select category_id, category_name,package,input_type,input_constraint from vw_categories where ic=@ic and can_upload='yes' order by category_name",
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic; // Session["ic"];

            conn.Open();

            var CategoryList = new List<EgrantsCategories>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                CategoryList.Add(
                    new EgrantsCategories
                        {
                            category_id = rdr["category_id"]?.ToString(),
                            category_name = rdr["category_name"]?.ToString(),
                            package = rdr["package"]?.ToString(),
                            input_type = rdr["input_type"]?.ToString(),
                            input_constraint = rdr["input_constraint"]?.ToString()
                        });

            conn.Close();

            return CategoryList;
        }

        // to load max Categoryid
        /// <summary>
        ///     The get max categoryid.
        /// </summary>
        /// <param name="ic">The ic.</param>
        /// <returns>
        ///     The <see cref="int" /> .
        /// </returns>
        public static int GetMaxCategoryid(string ic)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            var cmd = new SqlCommand("select max(category_id) as max_category_id from vw_categories where ic=@ic", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            conn.Open();
            var MaxCategoryid = 0;
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                MaxCategoryid = Convert.ToInt32(rdr["max_category_id"]);

            conn.Close();

            return MaxCategoryid;
        }

        // to load sub category list
        /// <summary>
        ///     The load sub category list.
        /// </summary>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public static List<EgrantsSubCategories> LoadSubCategoryList()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            var cmd = new SqlCommand("select parent_category_id,sub_category_name FROM categories_subcat_lookup", conn);
            cmd.CommandType = CommandType.Text;

            conn.Open();

            var SubCategoryList = new List<EgrantsSubCategories>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                SubCategoryList.Add(
                    new EgrantsSubCategories
                        {
                            parent_category_id = rdr["parent_category_id"]?.ToString(), sub_category_name = rdr["sub_category_name"]?.ToString()
                        }
                );

            rdr.Close();
            conn.Close();

            return SubCategoryList;
        }

        // to return document information 
        /// <summary>
        ///     The get doc info.
        /// </summary>
        /// <param name="doc_id">The doc_id.</param>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public static List<DocumentInforamtion> GetDocInfo(int doc_id)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);

            // System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select document_name + ' (' + convert(varchar, document_date, 101) +')' as DocumentInfo from egrants where document_id = @document_id", conn);
            var cmd = new SqlCommand(
                "select admin_phs_org_code, serial_num, full_grant_num, appl_id, category_id, ISNULL(sub_category_name, null) as sub_category_name, document_id, document_name, convert(varchar, document_date, 101) as document_date from egrants where document_id = @document_id",
                conn
            );

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@document_id", SqlDbType.Int).Value = doc_id;
            conn.Open();

            var DocInfo = new List<DocumentInforamtion>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                DocInfo.Add(
                    new DocumentInforamtion
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
                        }
                );

            rdr.Close();
            conn.Close();

            return DocInfo;

            // var document_info = (string)cmd.ExecuteScalar();
            // conn.Close();  
            // return document_info;
        }

        // to create a new document and return new document_id
        /// <summary>
        ///     The get doc id.
        /// </summary>
        /// <param name="applid">The applid.</param>
        /// <param name="categoryid">The categoryid.</param>
        /// <param name="subcategory">The subcategory.</param>
        /// <param name="docdate">The docdate.</param>
        /// <param name="filetype">The filetype.</param>
        /// <param name="ic">The ic.</param>
        /// <param name="userid">The userid.</param>
        /// <returns>
        ///     The <see cref="String" /> .
        /// </returns>
        public static string GetDocID(int applid, int categoryid, string subcategory, DateTime docdate, string filetype, string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_doc_create", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ApplID", SqlDbType.Int).Value = applid;
            cmd.Parameters.Add("@CategoryID", SqlDbType.Int).Value = categoryid;
            cmd.Parameters.Add("@SubCategory", SqlDbType.VarChar).Value = subcategory;
            cmd.Parameters.Add("@DocDate", SqlDbType.DateTime).Value = docdate;
            cmd.Parameters.Add("@file_type", SqlDbType.VarChar).Value = filetype;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@DocumentID", SqlDbType.Int);
            cmd.Parameters["@DocumentID"].Direction = ParameterDirection.Output;
            conn.Open();

            var DataReader = cmd.ExecuteReader();

            DataReader.Close();
            conn.Close();

            var document_id = Convert.ToString(cmd.Parameters["@DocumentID"].Value);

            return document_id;
        }

        // load doc Transaction History
        /// <summary>
        ///     The load doc transaction history.
        /// </summary>
        /// <param name="transaction_type">The transaction_type.</param>
        /// <param name="person_id">The person_id.</param>
        /// <param name="start_date">The start_date.</param>
        /// <param name="end_date">The end_date.</param>
        /// <param name="date_range">The date_range.</param>
        /// <param name="ic">The ic.</param>
        /// <param name="userid">The userid.</param>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public static List<DocTransactionHistory> LoadDocTransactionHistory(
            string transaction_type,
            int person_id,
            string start_date,
            string end_date,
            string date_range,
            string ic,
            string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_management_doc_transaction_report", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@transaction_type", SqlDbType.VarChar).Value = transaction_type;
            cmd.Parameters.Add("@startdate", SqlDbType.VarChar).Value = start_date;
            cmd.Parameters.Add("@enddate", SqlDbType.VarChar).Value = end_date;
            cmd.Parameters.Add("@date_range", SqlDbType.VarChar).Value = date_range;
            cmd.Parameters.Add("@person_id", SqlDbType.Int).Value = person_id;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

            conn.Open();
            var EgrantsDocs = new List<DocTransactionHistory>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                EgrantsDocs.Add(
                    new DocTransactionHistory
                        {
                            transaction_type = rdr["transaction_type"]?.ToString(),
                            document_id = rdr["document_id"]?.ToString(),
                            full_grant_num = rdr["full_grant_num"]?.ToString(),
                            category_name = rdr["category_name"]?.ToString(),
                            person_name = rdr["person_name"]?.ToString(),
                            url = rdr["url"]?.ToString(),
                            transaction_date = rdr["transaction_date"]?.ToString()
                        }
                );

            rdr.Close();
            conn.Close();

            return EgrantsDocs;
        }

        /// <summary>
        ///     The load impac docs.
        /// </summary>
        /// <param name="act">The act.</param>
        /// <param name="appl_id">The appl_id.</param>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public static List<ImpacDocs> LoadImpacDocs(string act, int appl_id)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            var cmd = new SqlCommand("sp_web_egrants_impac_docs", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            // cmd.Parameters.Add("@image_server", System.Data.SqlDbType.VarChar).Value = image_server;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@appl_id", SqlDbType.Int).Value = appl_id;
            conn.Open();

            var ImpactDocs = new List<ImpacDocs>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                ImpactDocs.Add(
                    new ImpacDocs
                        {
                            tag = rdr["tag"]?.ToString(),
                            appl_id = rdr["appl_id"]?.ToString(),
                            full_grant_num = rdr["full_grant_num"]?.ToString(),
                            accepted_date = rdr["accepted_date"]?.ToString(),
                            category_name = rdr["category_name"]?.ToString(),
                            created_date = rdr["created_date"]?.ToString(),
                            url = rdr["url"]?.ToString()
                        }
                );

            conn.Close();

            return ImpactDocs;
        }

        /// <summary>
        ///     The load docs unidentified.
        /// </summary>
        /// <param name="imageserver">The imageserver.</param>
        /// <param name="userid">The userid.</param>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public static List<DocsUnidentified> LoadDocsUnidentified(string imageserver, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "SELECT document_id,convert(varchar, document_date, 101) as document_date, document_name,"
              + " created_by,convert(varchar, created_date, 101) as created_date, convert(varchar, qc_date, 101) as qc_date, category_id, @imageserver + url as url from egrants"
              + " where appl_id is null and qc_date is not null and parent_id is null and qc_userid=@operator",
                conn
            );

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@imageserver", SqlDbType.VarChar).Value = imageserver;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

            conn.Open();
            var UnidentifiedDocs = new List<DocsUnidentified>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                UnidentifiedDocs.Add(
                    new DocsUnidentified
                        {
                            document_id = rdr["document_id"]?.ToString(),
                            document_name = rdr["document_name"]?.ToString(),
                            document_date = rdr["document_date"]?.ToString(),
                            category_id = rdr["category_id"].ToString(),
                            created_date = rdr["created_date"]?.ToString(),
                            created_by = rdr["created_by"]?.ToString(),
                            qc_date = rdr["qc_date"]?.ToString(),
                            url = rdr["url"]?.ToString()
                        }
                );

            rdr.Close();
            conn.Close();

            return UnidentifiedDocs;
        }

        // load doc attachments
        /// <summary>
        ///     The load doc attachments.
        /// </summary>
        /// <param name="document_id">The document_id.</param>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public static List<DocAttachment> LoadDocAttachments(int document_id)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("select url, document_name FROM vw_attachments WHERE document_id=@document_id", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@document_id", SqlDbType.Int).Value = document_id;

            conn.Open();
            var DocAttachments = new List<DocAttachment>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                DocAttachments.Add(new DocAttachment { document_name = rdr["document_name"]?.ToString(), url = rdr["url"]?.ToString() });

            rdr.Close();
            conn.Close();

            return DocAttachments;
        }

        /// <summary>
        ///     The get closeout notif.
        /// </summary>
        /// <param name="applid">The applid.</param>
        /// <param name="notifName">The notif name.</param>
        /// <returns>
        ///     The <see cref="EgrantsDoc.Notification" /> .
        /// </returns>
        public static Notification getCloseoutNotif(string applid, string notifName)
        {
            var cert_url = ConfigurationManager.ConnectionStrings["certPath"].ToString();
            var cert_pass = ConfigurationManager.ConnectionStrings["certPass"].ToString();
            var certificate = new X509Certificate2(cert_url, cert_pass);
            var request = (HttpWebRequest)WebRequest.Create(@"https://s2s.era.nih.gov/grantfolder/services/GrantDocumentInfo");
            request.ContentType = "application/xml";
            request.Method = "POST";
            request.ClientCertificates.Add(certificate);

            var SOAPReqBody = new XmlDocument();

            SOAPReqBody.LoadXml(
                @"<?xml version=""1.0"" encoding=""utf-8""?>  
                    <soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""
                    xmlns:mes=""http://era.nih.gov/grantDocumentInfo/message""> 
                    <soap:Header/> 
                    <soap:Body>
                    <mes:GrantCorrespondenceRequest>
                    <mes:applId>" + applid + @"</mes:applId>               
                    </mes:GrantCorrespondenceRequest> 
                    </soap:Body>
                    </soap:Envelope>"
            );

            using (var stream = request.GetRequestStream())
            {
                SOAPReqBody.Save(stream);
            }

            using (var Serviceres = request.GetResponse())
            {
                using (var rd = new StreamReader(Serviceres.GetResponseStream()))
                {
                    var ServiceResult = rd.ReadToEnd();
                    var pos = ServiceResult.IndexOf("apache.org>") + "apache.org>".Length;
                    ServiceResult = ServiceResult.Substring(pos);
                    pos = ServiceResult.IndexOf("--uuid:");
                    ServiceResult = ServiceResult.Substring(0, pos);
                    var doc = XDocument.Parse(ServiceResult);

                    // return doc;
                    XNamespace ns2 = "http://era.nih.gov/grantDocumentInfo/domain";
                    var responses = doc.Descendants(ns2 + "correspondenceData");
                    string notif_name = null;
                    var notif = new Notification();

                    foreach (var response in responses)
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
                            var mailbody = (string)response.Element(ns2 + "emailContent");

                            // mailbody = mailbody.Replace("<pre>", "");
                            // mailbody = mailbody.Replace("</pre>", "");
                            notif.emailContent = mailbody;
                        }
                    }

                    return notif;
                }
            }
        }

        /// <summary>
        ///     The former_appls.
        /// </summary>
        public class former_appls
        {
            /// <summary>
            ///     Gets or sets the former_num.
            /// </summary>
            public string former_num { get; set; }

            /// <summary>
            ///     Gets or sets the former_appl_id.
            /// </summary>
            public string former_appl_id { get; set; }
        }

        /// <summary>
        ///     The supplement.
        /// </summary>
        public class supplement
        {
            /// <summary>
            ///     Gets or sets the tag.
            /// </summary>
            public string tag { get; set; }

            /// <summary>
            ///     Gets or sets the id.
            /// </summary>
            public string id { get; set; }

            /// <summary>
            ///     Gets or sets the grant_id.
            /// </summary>
            public string grant_id { get; set; }

            /// <summary>
            ///     Gets or sets the serial_num.
            /// </summary>
            public string serial_num { get; set; }

            /// <summary>
            ///     Gets or sets the full_grant_num.
            /// </summary>
            public string full_grant_num { get; set; }

            /// <summary>
            ///     Gets or sets the former_appl_id.
            /// </summary>
            public string former_appl_id { get; set; }

            /// <summary>
            ///     Gets or sets the supp_appl_id.
            /// </summary>
            public string supp_appl_id { get; set; }

            /// <summary>
            ///     Gets or sets the support_year.
            /// </summary>
            public string support_year { get; set; }

            /// <summary>
            ///     Gets or sets the suffix_code.
            /// </summary>
            public string suffix_code { get; set; }

            /// <summary>
            ///     Gets or sets the former_num.
            /// </summary>
            public string former_num { get; set; }

            /// <summary>
            ///     Gets or sets the date_of_submitted.
            /// </summary>
            public string date_of_submitted { get; set; }

            /// <summary>
            ///     Gets or sets the category_name.
            /// </summary>
            public string category_name { get; set; }

            /// <summary>
            ///     Gets or sets the sub_category_name.
            /// </summary>
            public string sub_category_name { get; set; }

            /// <summary>
            ///     Gets or sets the status.
            /// </summary>
            public string status { get; set; }

            /// <summary>
            ///     Gets or sets the url.
            /// </summary>
            public string url { get; set; }

            /// <summary>
            ///     Gets or sets the moved_date.
            /// </summary>
            public string moved_date { get; set; }

            /// <summary>
            ///     Gets or sets the moved_by.
            /// </summary>
            public string moved_by { get; set; }

            /// <summary>
            ///     Gets or sets the accession_number.
            /// </summary>
            public string accession_number { get; set; }
        }

        /// <summary>
        ///     The egrants categories.
        /// </summary>
        public class EgrantsCategories
        {
            /// <summary>
            ///     Gets or sets the category_id.
            /// </summary>
            public string category_id { get; set; }

            /// <summary>
            ///     Gets or sets the category_name.
            /// </summary>
            public string category_name { get; set; }

            /// <summary>
            ///     Gets or sets the package.
            /// </summary>
            public string package { get; set; }

            /// <summary>
            ///     Gets or sets the input_type.
            /// </summary>
            public string input_type { get; set; }

            /// <summary>
            ///     Gets or sets the input_constraint.
            /// </summary>
            public string input_constraint { get; set; }
        }

        /// <summary>
        ///     The egrants sub categories.
        /// </summary>
        public class EgrantsSubCategories
        {
            /// <summary>
            ///     Gets or sets the parent_category_id.
            /// </summary>
            public string parent_category_id { get; set; }

            /// <summary>
            ///     Gets or sets the sub_category_name.
            /// </summary>
            public string sub_category_name { get; set; }
        }

        /// <summary>
        ///     The document inforamtion.
        /// </summary>
        public class DocumentInforamtion
        {
            /// <summary>
            ///     Gets or sets the admin_phs_org_code.
            /// </summary>
            public string admin_phs_org_code { get; set; }

            /// <summary>
            ///     Gets or sets the serial_num.
            /// </summary>
            public string serial_num { get; set; }

            /// <summary>
            ///     Gets or sets the full_grant_num.
            /// </summary>
            public string full_grant_num { get; set; }

            /// <summary>
            ///     Gets or sets the appl_id.
            /// </summary>
            public string appl_id { get; set; }

            /// <summary>
            ///     Gets or sets the category_id.
            /// </summary>
            public string category_id { get; set; }

            /// <summary>
            ///     Gets or sets the sub_category_name.
            /// </summary>
            public string sub_category_name { get; set; }

            /// <summary>
            ///     Gets or sets the document_id.
            /// </summary>
            public string document_id { get; set; }

            /// <summary>
            ///     Gets or sets the document_date.
            /// </summary>
            public string document_date { get; set; }

            /// <summary>
            ///     Gets or sets the document_name.
            /// </summary>
            public string document_name { get; set; }
        }

        /// <summary>
        ///     The doc transaction history.
        /// </summary>
        public class DocTransactionHistory
        {
            /// <summary>
            ///     Gets or sets the transaction_type.
            /// </summary>
            public string transaction_type { get; set; }

            /// <summary>
            ///     Gets or sets the document_id.
            /// </summary>
            public string document_id { get; set; }

            /// <summary>
            ///     Gets or sets the full_grant_num.
            /// </summary>
            public string full_grant_num { get; set; }

            /// <summary>
            ///     Gets or sets the category_name.
            /// </summary>
            public string category_name { get; set; }

            /// <summary>
            ///     Gets or sets the person_name.
            /// </summary>
            public string person_name { get; set; }

            /// <summary>
            ///     Gets or sets the url.
            /// </summary>
            public string url { get; set; }

            /// <summary>
            ///     Gets or sets the transaction_date.
            /// </summary>
            public string transaction_date { get; set; }
        }

        /// <summary>
        ///     The impac docs.
        /// </summary>
        public class ImpacDocs
        {
            /// <summary>
            ///     Gets or sets the tag.
            /// </summary>
            public string tag { get; set; }

            /// <summary>
            ///     Gets or sets the appl_id.
            /// </summary>
            public string appl_id { get; set; }

            /// <summary>
            ///     Gets or sets the full_grant_num.
            /// </summary>
            public string full_grant_num { get; set; }

            /// <summary>
            ///     Gets or sets the accepted_date.
            /// </summary>
            public string accepted_date { get; set; }

            /// <summary>
            ///     Gets or sets the category_name.
            /// </summary>
            public string category_name { get; set; }

            /// <summary>
            ///     Gets or sets the created_date.
            /// </summary>
            public string created_date { get; set; }

            /// <summary>
            ///     Gets or sets the url.
            /// </summary>
            public string url { get; set; }
        }

        /// <summary>
        ///     The docs unidentified.
        /// </summary>
        public class DocsUnidentified
        {
            /// <summary>
            ///     Gets or sets the document_id.
            /// </summary>
            public string document_id { get; set; }

            /// <summary>
            ///     Gets or sets the document_name.
            /// </summary>
            public string document_name { get; set; }

            /// <summary>
            ///     Gets or sets the document_date.
            /// </summary>
            public string document_date { get; set; }

            /// <summary>
            ///     Gets or sets the created_by.
            /// </summary>
            public string created_by { get; set; }

            /// <summary>
            ///     Gets or sets the created_date.
            /// </summary>
            public string created_date { get; set; }

            /// <summary>
            ///     Gets or sets the category_id.
            /// </summary>
            public string category_id { get; set; }

            /// <summary>
            ///     Gets or sets the qc_date.
            /// </summary>
            public string qc_date { get; set; }

            /// <summary>
            ///     Gets or sets the url.
            /// </summary>
            public string url { get; set; }
        }

        /// <summary>
        ///     The doc attachment.
        /// </summary>
        public class DocAttachment
        {
            /// <summary>
            ///     Gets or sets the document_name.
            /// </summary>
            public string document_name { get; set; }

            /// <summary>
            ///     Gets or sets the url.
            /// </summary>
            public string url { get; set; }
        }

        /// <summary>
        ///     The notification.
        /// </summary>
        public class Notification
        {
            /// <summary>
            ///     Gets or sets the notification name.
            /// </summary>
            public string notificationName { get; set; }

            /// <summary>
            ///     Gets or sets the id.
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            ///     Gets or sets the description.
            /// </summary>
            public string description { get; set; }

            /// <summary>
            ///     Gets or sets the sent date.
            /// </summary>
            public string sentDate { get; set; }

            /// <summary>
            ///     Gets or sets the from address.
            /// </summary>
            public string fromAddress { get; set; }

            /// <summary>
            ///     Gets or sets the to address.
            /// </summary>
            public string toAddress { get; set; }

            /// <summary>
            ///     Gets or sets the cc address.
            /// </summary>
            public string ccAddress { get; set; }

            /// <summary>
            ///     Gets or sets the subject.
            /// </summary>
            public string subject { get; set; }

            /// <summary>
            ///     Gets or sets the email content.
            /// </summary>
            public string emailContent { get; set; }
        }
    }
}