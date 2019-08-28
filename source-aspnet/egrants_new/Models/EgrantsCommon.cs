using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Helpers;
using egrants_new.Models;

namespace egrants_new.Models
{
    public class EgrantsCommon
    {

        //check user validation
        public static int CheckUserValidation(string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_user_validation", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();
            //int count= (int)cmd.ExecuteScalar();
            int count = 0;
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                count = Convert.ToInt16(rdr["count"]);
            }

            rdr.Close();
            conn.Close();
            return count;
        }

        //check user validation
        public static int CheckUsersException(string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_user_exception", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;

            conn.Open();
            //int count= (int)cmd.ExecuteScalar();
            int count = 0;
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                count = Convert.ToInt16(rdr["count"]);
            }

            rdr.Close();
            conn.Close();
            return count;
        }


        //check user application type
        public static string UserType(string ic, string userid)
        {         
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_egrants_user_type_check", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@Operator", System.Data.SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@user_application_type", System.Data.SqlDbType.VarChar, 2);
            cmd.Parameters["@user_application_type"].Direction = System.Data.ParameterDirection.Output;   
            conn.Open();

            System.Data.SqlClient.SqlDataReader DataReader = cmd.ExecuteReader();

            DataReader.Close();
            conn.Close();

            var application_type = Convert.ToString(cmd.Parameters["@user_application_type"].Value);

            return application_type;
        }

        public class User
        {
            public int personID { get; set; }
            public int positionID { get; set; }
            public int isCoordinator { get; set; }
            public string PersonName { get; set; }
            public string UserId { get; set; }
            public string PersonEmail { get; set; }
            public string Validation { get; set; }
            public string menulist { get; set; }
            public string ic { get; set; }
        }


        public class SMVariable
        {
            public string loginid { get; set; }
            public string ic { get; set; }
        }

        public static IEnumerable<User> uservar(string userid, string ic, string type)
        {

            string connectionString = ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString;
            List<User> users = new List<User>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_web_egrants_user_profile", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ic", ic);
                cmd.Parameters.AddWithValue("@Operator", userid);
                cmd.Parameters.AddWithValue("@type", type);

                conn.Open();
                User user = new User();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        if (rdr["nam"].ToString() == "VALIDATION")
                            user.Validation = rdr["val"].ToString();
                        if (rdr["nam"].ToString() == "USERID")
                            user.UserId = rdr["val"].ToString();
                        if (rdr["nam"].ToString() == "IC")
                            user.ic = rdr["val"].ToString();
                        if (rdr["nam"].ToString() == "PERSONID")
                            user.personID = Convert.ToInt32(rdr["val"]);
                        if (rdr["nam"].ToString() == "POSITIONID")
                            user.positionID = Convert.ToInt32(rdr["val"]);
                        if (rdr["nam"].ToString() == "ISCOORDINATOR")
                            user.isCoordinator = Convert.ToInt32(rdr["val"]);
                        if (rdr["nam"].ToString() == "USERNAME")
                            user.PersonName = rdr["val"].ToString();
                        if (rdr["nam"].ToString() == "USEREMAIL")
                            user.PersonEmail = rdr["val"].ToString();
                        if (rdr["nam"].ToString() == "MENULIST")
                            user.menulist = rdr["val"].ToString();
                    }
                }
                users.Add(user);
                return users;
            }
        }

        public class EgrantsUsers
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
            public string can_dashboard { get; set; }
            public string can_coordinator { get; set; }
            public string can_docman { get; set; }
            public string can_cft { get; set; }
            public string can_mgt { get; set; }
            public string can_iccoord { get; set; }
            public string start_date { get; set; }
            public string end_date { get; set; }
            public string create_date { get; set; }
            public string create_by { get; set; }
            public string last_update_date { get; set; }
            public string last_update_by { get; set; }
        }

        public static List<EgrantsCommon.EgrantsUsers> LoadSpecialists(string ic)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT person_name, person_id FROM vw_people WHERE ic=@IC and application_type='egrants' and position_id>1 and PATINDEX('%,%', person_name)>0 ORDER BY person_name", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;     // Session["ic"]; 
            conn.Open();

            var Specialists = new List<EgrantsCommon.EgrantsUsers>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Specialists.Add(new EgrantsCommon.EgrantsUsers
                {
                    person_id = rdr["person_id"].ToString(),
                    person_name = rdr["person_name"].ToString()
                });
            }
            conn.Close();
            return Specialists;
        }
     
        public class positions
        {
            public string position_id { get; set; }
            public string position_name { get; set; }
        }

        //load postions
        public static List<EgrantsCommon.positions> LoadPositions()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select position_id, position_name from people_positions order by position_id", conn);
            cmd.CommandType = CommandType.Text;
            conn.Open();

            var Positions = new List<EgrantsCommon.positions>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Positions.Add(new EgrantsCommon.positions
                {
                    position_id = rdr["position_id"].ToString(),
                    position_name = rdr["position_name"].ToString()
                });
            }
            conn.Close();
            return Positions;
        }

        //load Coordinators
        public static List<EgrantsCommon.EgrantsUsers> LoadCoordinators()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select person_id, person_name from vw_people where is_coordinator=1 order by person_name", conn);
            cmd.CommandType = CommandType.Text;
            conn.Open();

            var Coordinators = new List<EgrantsCommon.EgrantsUsers>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Coordinators.Add(new EgrantsCommon.EgrantsUsers
                {
                    person_id = rdr["person_id"]?.ToString(),
                    person_name = rdr["person_name"]?.ToString(),
                });
            }
            conn.Close();
            return Coordinators;
        }

        public class AdminCodes
        {
            public string profile { get; set; }
            public string admin_phs_org_code { get; set; }
        }

        public static List<AdminCodes> LoadAdminCodes()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);          
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select distinct admin_phs_org_code, case when admin_phs_org_code = 'ca' then 'NCI' else null end as profile " +
            " from grants ORDER BY admin_phs_org_code", conn);

            //System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT DISTINCT a.admin_phs_org_code, p.profile as profile " +
            //" FROM vw_appls a LEFT OUTER JOIN profiles p ON a.admin_phs_org_code = p.admin_phs_org_code ORDER BY a.admin_phs_org_code", conn);

            cmd.CommandType = CommandType.Text;
            conn.Open();

            var AdminCodes = new List<AdminCodes>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                AdminCodes.Add(new AdminCodes
                {
                    profile = rdr["profile"]?.ToString(),
                    admin_phs_org_code = rdr["admin_phs_org_code"]?.ToString(),
                });
            }
            conn.Close();
            return AdminCodes;
        } 

        public class CharacterIndex
        {
            public string index_id { get; set; }
            public string character_index { get; set; }
            public string index_seq { get; set; }
        }

        public static List<CharacterIndex> LoadCharacterIndex()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT index_id, character_index, index_seq from dbo.character_index where index_id>1 order by index_seq", conn);
            cmd.CommandType = CommandType.Text;
            conn.Open();

            var CharacterIndexs = new List<CharacterIndex>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                CharacterIndexs.Add(new CharacterIndex
                {
                    index_id = rdr["index_id"]?.ToString(),
                    character_index = rdr["character_index"]?.ToString(),
                    index_seq = rdr["index_seq"]?.ToString()
                });
            }
            conn.Close();
            return CharacterIndexs;
        }

        //load profile list
        public class Profiles
        {
            public string profile_id { get; set; }
            public string profile { get; set; }
            public string admin_phs_org_code { get; set; }
        }

        //load profile list
        public static List<Profiles> LoadProfiles()
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select profile_id, [profile], admin_phs_org_code from profiles order by admin_phs_org_code", conn);
            cmd.CommandType = CommandType.Text;
            conn.Open();

            var Profiles = new List<Profiles>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Profiles.Add (new Profiles
                {
                    profile_id = rdr["profile_id"]?.ToString(),
                    profile = rdr["profile"]?.ToString(),
                    admin_phs_org_code = rdr["admin_phs_org_code"]?.ToString()
                });        
            }
            conn.Close();
            return Profiles;
        }

        public class AdminMenus
        {
            public string menu_id { get; set; }
            public string menu_title { get; set; }
            public string menu_action { get; set; }
        }

        //load admin menu list
        public static List<AdminMenus> LoadAdminMenu(string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select menu_id, menu_title, menu_action from vw_adm_menu_assignment where person_id=(select person_id from vw_people where menu_action is not null and userid = @userid) order by menu_title", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@userid", System.Data.SqlDbType.VarChar).Value =userid; 
            conn.Open();

            var AdminMenu = new List<AdminMenus>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                AdminMenu.Add(new AdminMenus {
                    menu_id = rdr["menu_id"]?.ToString(),
                    menu_title = rdr["menu_title"]?.ToString(),
                    menu_action = rdr["menu_action"]?.ToString()                  
                });
            }
            conn.Close();
            return AdminMenu;
        }

        //get person_id with userid
        public static int GetPersonID(string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select person_id from vw_people where userid=@userid", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@userid", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            int person_id = 0;
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                person_id = Convert.ToInt16(rdr["person_id"]);
            }

            rdr.Close();
            conn.Close();
            return person_id;
        }

        //check uesr run  Admin Menu Permission by userid and menu_id
        public static int CheckAdminMenuPermission(int menu_id, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select count(*) as Permission from vw_adm_menu_assignment where person_id=(select person_id from people where userid=@userid) and menu_id=@menu_id", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@menu_id", System.Data.SqlDbType.Int).Value = menu_id;
            cmd.Parameters.Add("@userid", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            int Permission=0; 
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Permission = Convert.ToInt16(rdr["Permission"]);         
            }
            conn.Close();
            return Permission;
        }

        public class Pagination
        {
            public string tag { get; set; }
            public string parent { get; set; }
            public string total_counts { get; set; }
            public string total_tabs { get; set; }
            public string total_pages { get; set; }
            public string tab_number { get; set; }
            public string page_number { get; set; }
        }
    }
}