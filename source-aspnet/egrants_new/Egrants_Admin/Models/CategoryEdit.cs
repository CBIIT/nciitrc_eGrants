using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;

namespace egrants_new.Egrants_Admin.Models
{
    public class CategoryEdit
    {
        public static List<Egrants.Models.EgrantsDoc.EgrantsCategories> LoadCommonCategroies(string ic)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select distinct ct.category_id, category_name from categories ct, categories_ic ci where ct.category_id = ci.category_id and ic <> @ic "+
            "Union select distinct ct.category_id, category_name from categories ct, categories_ic ci where ct.category_id = ci.category_id and ic = @ic and removed_date is not null order by category_name", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            conn.Open();

            var CommonCategroies = new List<Egrants.Models.EgrantsDoc.EgrantsCategories>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
                {
                    CommonCategroies.Add(new Egrants.Models.EgrantsDoc.EgrantsCategories
                    {
                        category_id = rdr["category_id"]?.ToString(),
                        category_name = rdr["category_name"]?.ToString()
                    });
                }
            rdr.Close();
            conn.Close();

            return CommonCategroies;
        }

        public static List<Egrants.Models.EgrantsDoc.EgrantsCategories> LoadLocalCategroies(string ic)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select distinct category_id, category_name from vw_categories where ic=@ic order by category_name", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            conn.Open();

            var LocalCategroies = new List<Egrants.Models.EgrantsDoc.EgrantsCategories>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
                {
                    LocalCategroies.Add(new Egrants.Models.EgrantsDoc.EgrantsCategories
                    {
                        category_id = rdr["category_id"]?.ToString(),
                        category_name = rdr["category_name"]?.ToString()
                    });
                }
            rdr.Close();
            conn.Close();

            return LocalCategroies;          
        }

        public static string run_db(string act, int category_id, string category_name, string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("dbo.sp_web_admin_category_edit", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", System.Data.SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@category_id", System.Data.SqlDbType.Int).Value = category_id;
            cmd.Parameters.Add("@category_name", System.Data.SqlDbType.VarChar).Value = category_name;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            cmd.Parameters.Add("@return_notice", System.Data.SqlDbType.VarChar, 200);
            cmd.Parameters["@return_notice"].Direction = System.Data.ParameterDirection.Output;
            conn.Open();
            System.Data.SqlClient.SqlDataReader DataReader = cmd.ExecuteReader();
            DataReader.Close();
            conn.Close();

            var return_message = Convert.ToString(cmd.Parameters["@return_notice"].Value);
            return return_message;
        }
    }
}