using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;

namespace egrants_new.Models
{
    public class DocmanCommon
    {
        //public class User
        //{
        //    public int personID { get; set; }
        //    public int positionID { get; set; }
        //    public string PersonName { get; set; }
        //    public string UserId { get; set; }
        //    public string PersonEmail { get; set; }
        //    public string Validation { get; set; }
        //    public string menulist { get; set; }
        //    public string ic { get; set; }
        //}


        //public class SMVariable
        //{
        //    public string loginid { get; set; }
        //    public string ic { get; set; }
        //}

        //public static IEnumerable<User> uservar(string userid, string ic)
        //{

        //    string connectionString = ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString;
        //    List<User> users = new List<User>();
        //    using (SqlConnection conn = new SqlConnection(connectionString))
        //    {
        //        SqlCommand cmd = new SqlCommand("sp_web_docman_user_detail", conn);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Parameters.AddWithValue("@ic", ic);
        //        cmd.Parameters.AddWithValue("@usrid", userid);

        //        conn.Open();
        //        User user = new User();
        //        using (SqlDataReader rdr = cmd.ExecuteReader())
        //        {
        //            while (rdr.Read())
        //            {
        //                if (rdr["nam"].ToString() == "VALIDATION")
        //                    user.Validation = rdr["val"].ToString();
        //                if (rdr["nam"].ToString() == "USERID")
        //                    user.UserId = rdr["val"].ToString();
        //                if (rdr["nam"].ToString() == "IC")
        //                    user.ic = rdr["val"].ToString();
        //                if (rdr["nam"].ToString() == "PERSONID")
        //                    user.personID = Convert.ToInt32(rdr["val"]);
        //                if (rdr["nam"].ToString() == "POSITIONID")
        //                    user.positionID = Convert.ToInt32(rdr["val"]);
        //                if (rdr["nam"].ToString() == "USERNAME")
        //                    user.PersonName = rdr["val"].ToString();
        //                if (rdr["nam"].ToString() == "USEREMAIL")
        //                    user.PersonEmail = rdr["val"].ToString();
        //                if (rdr["nam"].ToString() == "MENULIST")
        //                    user.menulist = rdr["val"].ToString();
        //            }
        //        }
        //        users.Add(user);
        //        return users;
        //    }
        //}

        public class DocmanContract
        {
            public string econ_id { get; set; }
            public string piid { get; set; }
            public string ref_piid { get; set; }
            public string combined_piid { get; set; }
            public string award_mod_num { get; set; }
            public string rfp_number { get; set; }
            public string full_contract_number { get; set; }
            public string person_id { get; set; }
            public string specialist_id { get; set; }
            public string specialist_name { get; set; }
            public string team_leader { get; set; }
            public string branch_chief { get; set; }
            public string institution { get; set; }
            public string project_enddate { get; set; }
            public string project_startdate { get; set; }
            public string created_date { get; set; }
            public string close_out { get; set; }
            public string fiscal_year { get; set; }
            public string can_upload { get; set; }
            public string document_count { get; set; }
            public string page_count { get; set; }
        }
        public static List<DocmanContract> LoadDocmanContract(string str, int econ_id, int page_index, string browser, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_docman_search", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@str", System.Data.SqlDbType.VarChar).Value = str;
            cmd.Parameters.Add("@econ_id", System.Data.SqlDbType.Int).Value = econ_id;
            cmd.Parameters.Add("@page_index", System.Data.SqlDbType.Int).Value = page_index;
            cmd.Parameters.Add("@browser", System.Data.SqlDbType.VarChar).Value = browser;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            var DocmanContract = new List<DocmanContract>();
            conn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                DocmanContract.Add(new DocmanContract
                {
                    econ_id = rdr["econ_id"]?.ToString(),
                    piid = rdr["piid"]?.ToString(),
                    ref_piid = rdr["ref_piid"]?.ToString(),
                    full_contract_number = rdr["full_contract_number"]?.ToString(),
                    award_mod_num = rdr["award_mod_num"]?.ToString(),
                    rfp_number = rdr["rfp_number"]?.ToString(),
                    institution = rdr["vendor_name"]?.ToString(),
                    project_enddate = rdr["project_enddate"]?.ToString(),
                    close_out = rdr["close_out"]?.ToString(),
                    person_id = rdr["person_id"]?.ToString(),
                    specialist_name = rdr["specialist_name"]?.ToString(),
                    team_leader = rdr["team_leader"]?.ToString(),
                    branch_chief = rdr["branch_chief"]?.ToString(),
                    can_upload = rdr["can_upload"]?.ToString(),
                    document_count = rdr["document_count"]?.ToString(),
                    page_count = rdr["page_count"]?.ToString()
                });
            }
            conn.Close();
            return DocmanContract;
        }

        //load Docman contract for CFT project only
        public static List<DocmanContract> LoadAdditionalContract(int serial_num)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT contract_number,fiscal_year,dbo.fn_clean_characters(institution) AS institution," +
            " contract_type, activity_code, upper(activity_code) + '-' + upper(contract_type) + '-' + CONVERT(varchar, right(contract_number, 5)) as full_contract_number," +
            " piid, rfp, specialist_name, upper(close_out) AS close_out, combined_piid FROM dbo.vw_CFT_I2EContracts " +
            " WHERE piid is not null and contract_number = @serial_num", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@serial_num", System.Data.SqlDbType.Int).Value = serial_num;

            var DocmanContracts = new List<DocmanContract>();
            conn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                DocmanContracts.Add(new DocmanContract
                {
                    piid = rdr["piid"]?.ToString(),
                    combined_piid = rdr["combined_piid"]?.ToString(),
                    specialist_name = rdr["specialist_name"]?.ToString(),
                    full_contract_number = rdr["full_contract_number"]?.ToString(),
                    institution = rdr["institution"]?.ToString(),
                    close_out = rdr["close_out"]?.ToString(),
                    fiscal_year = rdr["fiscal_year"]?.ToString()
                });
            }

            conn.Close();

            return DocmanContracts;
        }

        public class MainCategories
        {
            public string mcat_id { get; set; }
            public string mcat_name { get; set; }
            public string mcat_sort_order { get; set; }
            public string mcat_label { get; set; }
        }

        public static List<MainCategories> LoadMainCategories()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select mcat_id, mcat_name, label, sort_order from dbo.DocMan_Main_categories where End_date is null ORDER BY mcat_id", conn);
            cmd.CommandType = CommandType.Text;

            var MainCategorieList = new List<MainCategories>();
            conn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                MainCategorieList.Add(new MainCategories
                {
                    mcat_id = rdr["mcat_id"]?.ToString(),
                    mcat_name = rdr["mcat_name"]?.ToString(),
                    mcat_label = rdr["label"]?.ToString(),
                    mcat_sort_order = rdr["sort_order"]?.ToString(),
                });
            }
            conn.Close();
            return MainCategorieList;
        }

        public static int GetMCatTotal()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select count(*) as mcat_total from dbo.DocMan_Main_categories where end_date is null", conn);
            cmd.CommandType = CommandType.Text;
            int MCateTotal= 0;
            conn.Open(); 
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                MCateTotal = Convert.ToInt16(rdr["mcat_total"]);
            }
            conn.Close();
            return MCateTotal;
        }

        public class SubCategories
        {
            public string mcat_id { get; set; }
            public string ccat_id { get; set; }
            public string ccat_name { get; set; }
            public string ccat_sort_order { get; set; }
            public string ccat_label { get; set; }
        }
        public static List<SubCategories> LoadSubCategories()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select Mcat_id, Ccat_id, Ccat_name,label, sort_order from dbo.DocMan_Child_categories where end_dt is null order by Mcat_id, Ccat_id", conn);
            cmd.CommandType = CommandType.Text;

            var SubCategorieList = new List<SubCategories>();
            conn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                SubCategorieList.Add(new SubCategories
                {
                    mcat_id = rdr["mcat_id"]?.ToString(),
                    ccat_id = rdr["ccat_id"]?.ToString(),
                    ccat_name = rdr["ccat_name"]?.ToString(),
                    ccat_label = rdr["label"]?.ToString(),
                    ccat_sort_order = rdr["sort_order"]?.ToString(),
                });
            }
            conn.Close();
            return SubCategorieList;
        }

        public static int GetdCCatTotal()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select count(*) as ccat_total from dbo.DocMan_Child_Categories where End_dt is null", conn);
            cmd.CommandType = CommandType.Text;
            int CCateTotal = 0;
            conn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                CCateTotal = Convert.ToInt16(rdr["ccat_total"]);
            }
            conn.Close();
            return CCateTotal;
        }

        public class DocmanDeletedDocuments
        {
            public string docment_id { get; set; }
            public string econ_id { get; set; }
            public string piid { get; set; }
            public string ref_piid { get; set; }
            public string ccat_name { get; set; }
            public string url_loc { get; set; }
            public string report_label { get; set; }
            public string disabled_date { get; set; }
            public string disabled_reason { get; set; }
            public string disabled_by { get; set; }
        }

        public class DocmantAccessions
        {
            public string accession_id { get; set; }
            public string accession_number { get; set; }
            public string accession_year { get; set; }
            public string accession_counter { get; set; }
        }

        public static List<DocmantAccessions> LoadAccessions()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT distinct accession_id,accession_number from vw_boxes order by accession_id desc", conn);
            cmd.CommandType = CommandType.Text;
            conn.Open();

            var Accessions = new List<DocmantAccessions>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Accessions.Add(new DocmantAccessions
                {
                    accession_id = rdr["accession_id"].ToString(),
                    accession_number = rdr["accession_number"].ToString()
                });
            }
            conn.Close();
            return Accessions;
        }

        public static List<DocmanUsers> LoadCoordinators()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select person_id, person_name from people where is_coordinator=1 and active=1 order by person_name", conn);
            cmd.CommandType = CommandType.Text;
            conn.Open();

            var Coordinators = new List<DocmanUsers>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Coordinators.Add(new DocmanUsers
                {
                    person_id = rdr["person_id"]?.ToString(),
                    person_name = rdr["person_name"]?.ToString(),
                });
            }
            conn.Close();
            return Coordinators;
        }
        public class DocmanUsers
        {
            public string character_index { get; set; }
            public string person_id { get; set; }
            public string person_name { get; set; }
            public string userid { get; set; }
            public string first_name { get; set; }
            public string middle_name { get; set; }
            public string last_name { get; set; }
            public string phone_number { get; set; }
            public string email_address { get; set; }
            public string profile_id { get; set; }
            public string position_id { get; set; }
            public string position_name { get; set; }
            public string active { get; set; }
            public string ic { get; set; }
            public string application_type { get; set; }
            public string is_coordinator { get; set; }
            public string coordinator_id { get; set; }
            public string can_admin { get; set; }
            public string can_egrants { get; set; }
            public string can_docman { get; set; }
            public string can_cft { get; set; }
            public string start_date { get; set; }
            public string end_date { get; set; }
            public string create_date { get; set; }
            public string create_by { get; set; }
            public string last_update_date { get; set; }
            public string last_update_by { get; set; }
        }

        public static List<DocmanUsers> LoadDocmanUsers(string ic)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["DocmanDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select person_id, person_name from vw_people "
            + "where ic = @ic and application_type = 'econtracts' and (person_name is not null and person_name <> '') order by person_name", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            conn.Open();

            var DocmanUsers = new List<DocmanUsers>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                DocmanUsers.Add(new DocmanUsers
                {
                    person_id = rdr["person_id"]?.ToString(),
                    person_name = rdr["person_name"]?.ToString()
                });
            }
            conn.Close();
            return DocmanUsers;
        }

        //public class CFTContract
        //{
        //    public string contract_id { get; set; }
        //    public string contract_number { get; set; }
        //    public string contract_type { get; set; }
        //    public string activity_code { get; set; }
        //    public string full_contract_number { get; set; }
        //    public string piid { get; set; }
        //    public string fiscal_year { get; set; }
        //    public string institution { get; set; }
        //    public string rfp { get; set; }
        //    public string specialist_name { get; set; }
        //    public string close_out { get; set; }
        //}

        public class DocmantBoxes
        {
            public string accession_id { get; set; }
            public string box_id { get; set; }
            public string box_number { get; set; }
            public string box { get; set; }
        }
    }
}


