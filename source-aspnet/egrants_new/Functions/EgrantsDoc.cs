using egrants_new.Egrants.Models;
using egrants_new.Models;
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

namespace egrants_new.Functions
{
    /// <summary>
    ///     The egrants doc.
    /// </summary>
    public static class EgrantsDoc
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
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@grant_id", SqlDbType.Int).Value = grant_id;
            conn.Open();

            var list = new List<former_appls>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                list.Add(new former_appls { former_num = rdr["former_num"]?.ToString(), former_appl_id = rdr["former_appl_id"]?.ToString() });
            }

            conn.Close();

            return list;
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

            var list = new List<supplement>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                list.Add(
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
                    });
            }

            conn.Close();

            return list;
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

        // to load 
        /// <summary>
        ///     Get the load specialist listusers.
        /// </summary>
        /// <param name="ic">The ic.</param>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public static List<EgrantsUsers> LoadUsers(string ic)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            var cmd = new SqlCommand("select person_id, person_name from vw_people where position_id>1 and ic=@ic order by person_name", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;

            var list = new List<EgrantsUsers>();
            conn.Open();

            cmd.CommandTimeout = 120;
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                list.Add(new EgrantsUsers { PersonId = rdr["person_id"]?.ToString(), person_name = rdr["person_name"]?.ToString() });
            }

            conn.Close();

            return list;
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

            var list = new List<EgrantsCategories>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                list.Add(
                    new EgrantsCategories
                    {
                        category_id = rdr["category_id"]?.ToString(),
                        category_name = rdr["category_name"]?.ToString(),
                        package = rdr["package"]?.ToString(),
                        input_type = rdr["input_type"]?.ToString(),
                        input_constraint = rdr["input_constraint"]?.ToString()
                    });
            }

            conn.Close();

            return list;
        }

        /// <summary>
        ///     The get max categoryid.
        /// </summary>
        /// <param name="ic">The ic.</param>
        /// <returns>rasmu
        ///     The <see cref="int" /> .
        /// </returns>
        public static int GetMaxCategoryid(string ic)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            var cmd = new SqlCommand("select max(category_id) as max_category_id from vw_categories where ic=@ic", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            conn.Open();
            var maxCategoryid = 0;
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                maxCategoryid = Convert.ToInt32(rdr["max_category_id"]);
            }

            conn.Close();

            return maxCategoryid;
        }

        /// <summary>
        ///     Load sub category list.
        /// </summary>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public static List<EgrantsSubCategories> LoadSubCategoryList()
        {
            var list = new List<EgrantsSubCategories>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand("select parent_category_id,sub_category_name FROM categories_subcat_lookup", conn);
                cmd.CommandType = CommandType.Text;

                conn.Open();


                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(
                        new EgrantsSubCategories
                        {
                            parent_category_id = rdr["parent_category_id"]?.ToString(), sub_category_name = rdr["sub_category_name"]?.ToString()
                        });
                }
            }

            return list;
        }

        /// <summary>
        ///     Get document info
        /// </summary>
        /// <param name="doc_id">The doc_id.</param>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public static List<DocumentInforamtion> GetDocInfo(int doc_id)
        {
            var list = new List<DocumentInforamtion>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {

                // System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select document_name + ' (' + convert(varchar, document_date, 101) +')' as DocumentInfo from egrants where document_id = @document_id", conn);
                var cmd = new SqlCommand(
                    "select admin_phs_org_code, serial_num, full_grant_num, appl_id, category_id, ISNULL(sub_category_name, null) as sub_category_name, document_id, document_name, convert(varchar, document_date, 101) as document_date from egrants where document_id = @document_id",
                    conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@document_id", SqlDbType.Int).Value = doc_id;
                conn.Open();

                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(
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
                        });
                }
            }

            return list;
        }

        /// <summary>
        ///     Creates a new document and returns the new document_id.
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
            var list = new List<DocTransactionHistory>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                list.Add(
                    new DocTransactionHistory
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

            return list;
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

            var list = new List<ImpacDocs>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                list.Add(
                    new ImpacDocs
                    {
                        tag = rdr["tag"]?.ToString(),
                        appl_id = rdr["appl_id"]?.ToString(),
                        full_grant_num = rdr["full_grant_num"]?.ToString(),
                        accepted_date = rdr["accepted_date"]?.ToString(),
                        category_name = rdr["category_name"]?.ToString(),
                        created_date = rdr["created_date"]?.ToString(),
                        url = rdr["url"]?.ToString(),

                        // document_id = rdr["document_id"]?
                    });
            }

            conn.Close();

            return list;
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
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@imageserver", SqlDbType.VarChar).Value = imageserver;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;

            conn.Open();
            var list = new List<DocsUnidentified>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                list.Add(
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
                    });
            }

            rdr.Close();
            conn.Close();

            return list;
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
            var list = new List<DocAttachment>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                list.Add(new DocAttachment { document_name = rdr["document_name"]?.ToString(), url = rdr["url"]?.ToString() });
            }

            rdr.Close();
            conn.Close();

            return list;
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
            var certUrl = ConfigurationManager.ConnectionStrings["certPath"].ToString();
            var certPass = ConfigurationManager.ConnectionStrings["certPass"].ToString();
            var certificate = new X509Certificate2(certUrl, certPass);
            var eraUrl = ConfigurationManager.AppSettings["era_url_base"];
            var request = (HttpWebRequest)WebRequest.Create($"{eraUrl}grantfolder/services/GrantDocumentInfo");
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
                    </soap:Envelope>");

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
    }
}