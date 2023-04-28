#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  EgrantsAppl.cs
// Solution: egrants_new
// Project:  egrants_new
// Created: 2022-05-05
// Contributors:
//      - Briggs, Robin (NIH/NCI) [C] - briggsr2
//      -
// Copyright (c) National Institute of Health
// 
// <Description of the file>
// 
// This source is subject to the NIH Softwre License.
// See https://ncihub.org/resources/899/download/Guidelines_for_Releasing_Research_Software_04062015.pdf
// All other rights reserved.
// 
// THE SOFTWARE IS PROVIDED "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT ARE DISCLAIMED. IN NO EVENT SHALL THE NATIONAL
// CANCER INSTITUTE (THE PROVIDER), THE NATIONAL INSTITUTES OF HEALTH, THE
// U.S. GOVERNMENT OR THE INDIVIDUAL DEVELOPERS BE LIABLE FOR ANY DIRECT,
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
// \***************************************************************************/

#endregion

#region

using egrants_new.Egrants.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

#endregion

namespace egrants_new.Functions
{
    /// <summary>
    /// The egrants appl.
    /// </summary>
    public static class EgrantsAppl
    {

        // check if this appl_id is existing in appls table, added by Leon 7/10/2019
        /// <summary>
        /// The check appl id.
        /// </summary>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int CheckApplID(int appl_id)
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand("select count(*) as count_id from appls where appl_id = @appl_id", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@appl_id", SqlDbType.Int).Value = appl_id;

                conn.Open();
                var isexisting = 0;
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    isexisting = Convert.ToInt32(rdr["count_id"]);

                return isexisting;
            }
        }

        // to load Appl Type
        /// <summary>
        /// The load appl type.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<ApplType> LoadApplType()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            var cmd = new SqlCommand("select distinct appl_type_code from appls where appl_id > 0", conn);
            cmd.CommandType = CommandType.Text;

            var ApplTypeList = new List<ApplType>();
            conn.Open();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                ApplTypeList.Add(new ApplType { appl_type_code = rdr["appl_type_code"]?.ToString() });

            rdr.Close();
            conn.Close();

            return ApplTypeList;
        }

        // to load Activity Code
        /// <summary>
        /// The load activity code.
        /// </summary>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<ActivityCode> LoadActivityCode(string admin_code)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "select distinct activity_code from vw_appls where appl_id > 0 and admin_phs_org_code = @admin_code order by activity_code",
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@admin_code", SqlDbType.VarChar).Value = admin_code;

            conn.Open();

            var ActivityCodeList = new List<ActivityCode>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                ActivityCodeList.Add(new ActivityCode { activity_code = rdr["activity_code"]?.ToString() });

            rdr.Close();
            conn.Close();

            return ActivityCodeList;
        }

        // to create a new appl
        /// <summary>
        /// The create new appl.
        /// </summary>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <param name="appl_type">
        /// The appl_type.
        /// </param>
        /// <param name="activity_code">
        /// The activity_code.
        /// </param>
        /// <param name="support_year">
        /// The support_year.
        /// </param>
        /// <param name="suffix_code">
        /// The suffix_code.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string CreateNewAppl(
            string admin_code,
            int serial_num,
            int appl_type,
            string activity_code,
            int support_year,
            string suffix_code,
            string ic,
            string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            var cmd = new SqlCommand("dbo.sp_web_egrants_create_appl", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@admin_code", SqlDbType.VarChar).Value = admin_code;
            cmd.Parameters.Add("@serial_num", SqlDbType.Int).Value = serial_num;
            cmd.Parameters.Add("@appl_type_code", SqlDbType.Int).Value = appl_type;
            cmd.Parameters.Add("@activity_code", SqlDbType.VarChar).Value = activity_code;
            cmd.Parameters.Add("@support_year", SqlDbType.Int).Value = support_year;
            cmd.Parameters.Add("@suffix_code", SqlDbType.VarChar).Value = suffix_code;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@return_notice", SqlDbType.VarChar, 200);
            cmd.Parameters["@return_notice"].Direction = ParameterDirection.Output;

            conn.Open();
            var DataReader = cmd.ExecuteReader();
            DataReader.Close();
            conn.Close();

            var return_message = Convert.ToString(cmd.Parameters["@return_notice"].Value);

            return return_message;
        }

        // create an appl_id list string by year or flag_type
        /// <summary>
        /// The get appls list.
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <param name="flag_type">
        /// The flag_type.
        /// </param>
        /// <param name="years">
        /// The years.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetApplsList(int grant_id, string flag_type = null, string years = null)
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand("sp_web_egrants_load_applid_string", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@grant_id", SqlDbType.Int).Value = grant_id;
                cmd.Parameters.Add("@flag_type", SqlDbType.VarChar).Value = flag_type;
                cmd.Parameters.Add("@years", SqlDbType.VarChar).Value = years;

                conn.Open();
                var appls_list = (string)cmd.ExecuteScalar();


                return appls_list;
            }
        }

        // to load appls by admin_code and serial_num
        /// <summary>
        /// The load appls_by_serialnum.
        /// </summary>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Appl> LoadApplsBySerialnum(string admin_code, int serial_num)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "select appl_id, full_grant_num from vw_appls where admin_phs_org_code = @admincode and serial_num=@serialnum order by support_year desc",
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@admincode", SqlDbType.VarChar).Value = admin_code;
            cmd.Parameters.Add("@serialnum", SqlDbType.Int).Value = serial_num;
            conn.Open();

            var GrantYearList = new List<Appl>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                GrantYearList.Add(
                    new Appl
                        {
                            appl_id = rdr["appl_id"]?.ToString(),

                            // support_year = rdr["support_year"]?.ToString(),
                            full_grant_num = rdr["full_grant_num"]?.ToString()
                        });

            rdr.Close();
            conn.Close();

            return GrantYearList;
        }

        // to load appls by appl_id
        /// <summary>
        /// The load appls_by_grantid.
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Appl> LoadApplsByGrantId(int grant_id)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);
            var cmd = new SqlCommand("select appl_id, full_grant_num from vw_appls where grant_id=@grantid order by support_year desc", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@grantid", SqlDbType.Int).Value = grant_id;
            conn.Open();

            var ApplsList = new List<Appl>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                ApplsList.Add(new Appl { appl_id = rdr["appl_id"]?.ToString(), full_grant_num = rdr["full_grant_num"]?.ToString() });

            rdr.Close();
            conn.Close();

            return ApplsList;
        }

        // to load appls by appl_id
        /// <summary>
        /// The load appls_by_applid.
        /// </summary>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Appl> LoadApplsByApplid(int appl_id)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "select appl_id, support_year, full_grant_num from vw_appls where grant_id=(select grant_id from appls where appl_id=@applid) order by support_year desc",
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@applid", SqlDbType.Int).Value = appl_id;
            conn.Open();

            var GrantYearList = new List<Appl>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                GrantYearList.Add(
                    new Appl
                        {
                            appl_id = rdr["appl_id"]?.ToString(),
                            support_year = rdr["support_year"]?.ToString(),
                            full_grant_num = rdr["full_grant_num"]?.ToString()
                        });

            rdr.Close();
            conn.Close();

            return GrantYearList;
        }

        // get all appls by appl_id
        /// <summary>
        /// The get all appls.
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<string> GetAllAppls(int grant_id)
        {
            var applsList = new List<string>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand(
                    "select appl_id, full_grant_num, support_year from vw_appls where doc_count>0 and grant_id=@grant_id order by support_year desc",
                    conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@grant_id", SqlDbType.Int).Value = grant_id;

                conn.Open();
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    var applData = rdr[0] + ":" + rdr[1];
                    applsList.Add(applData);
                }
            }

            return applsList;
        }

        // get latest 12 appls by appl_id
        /// <summary>
        /// The get default appls.
        /// </summary>
        /// <param name="grant_id">
        /// The grant_id.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<string> GetDefaultAppls(int grant_id)
        {
            var applsList = new List<string>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand(
                    "select top 12 appl_id, full_grant_num, support_year from vw_appls where doc_count>0 and grant_id=@grant_id order by support_year desc",
                    conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@grant_id", SqlDbType.Int).Value = grant_id;

                conn.Open();
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    var applData = rdr[0] + ":" + rdr[1];
                    applsList.Add(applData);
                }
            }

            return applsList;
        }

        // load uploadable appls by admin_code and serial_num
        /// <summary>
        /// The load uploadable appls_by_serialnum.
        /// </summary>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Appl> LoadUploadableApplsBySerialnum(string admin_code, int serial_num)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "select appl_id, full_grant_num,support_year from vw_appls "
              + " where admin_phs_org_code = @admincode and serial_num=@serialnum and frc_destroyed=0 and deleted_by_impac='n' order by support_year desc",
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@admincode", SqlDbType.VarChar).Value = admin_code;
            cmd.Parameters.Add("@serialnum", SqlDbType.VarChar).Value = serial_num;
            conn.Open();

            var GrantYearList = new List<Appl>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                GrantYearList.Add(
                    new Appl
                        {
                            appl_id = rdr["appl_id"]?.ToString(),
                            support_year = rdr["support_year"]?.ToString(),
                            full_grant_num = rdr["full_grant_num"]?.ToString()
                        });

            rdr.Close();
            conn.Close();

            return GrantYearList;
        }

        // load uploadable appls by appl_id
        /// <summary>
        /// The load uploadable appls_by_applid.
        /// </summary>
        /// <param name="appl_id">
        /// The appl_id.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Appl> LoadUploadableApplsByApplid(int appl_id)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "select appl_id, support_year, full_grant_num from vw_appls "
              + " where grant_id = (select grant_id from appls where appl_id = @applid) and frc_destroyed=0 and deleted_by_impac='n' order by support_year desc",
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@applid", SqlDbType.Int).Value = appl_id;
            conn.Open();

            var GrantYearList = new List<Appl>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                GrantYearList.Add(
                    new Appl
                        {
                            appl_id = rdr["appl_id"]?.ToString(),
                            support_year = rdr["support_year"]?.ToString(),
                            full_grant_num = rdr["full_grant_num"]?.ToString()
                        });

            rdr.Close();
            conn.Close();

            return GrantYearList;
        }

        // load all appls with documents or without documents
        /// <summary>
        /// The get all appls list.
        /// </summary>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<string> GetAllApplsList(string admin_code, string serial_num)
        {
            var yearList = new List<string>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["egrantsDB"].ConnectionString))
            {
                var cmd = new SqlCommand(
                    "select full_grant_num, appl_id from vw_appls where admin_phs_org_code = @admincode and serial_num = @serialnum order by support_year desc",
                    conn);

                // SqlCommand cmd = new SqlCommand("select support_year_suffix from vw_appls where fy = @fy and activity_code = @mechan  and admin_phs_org_code = @ic and serial_num = @serialnum", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@admincode", admin_code);
                cmd.Parameters.AddWithValue("@serialnum", serial_num);

                conn.Open();
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    var applData = rdr[0] + ":" + rdr[1];
                    yearList.Add(applData);
                }
            }

            return yearList;
        }

    }
}