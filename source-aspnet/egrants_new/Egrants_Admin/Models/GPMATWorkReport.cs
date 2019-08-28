using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using egrants_new.Models;

namespace egrants_new.Egrants_Admin.Models
{
    public class GPMATWorkReport
    {

        public class PMATWorkReports
        { 
            public string specialist_name { get; set; }
            public string specialist_code { get; set; }
            public string branch { get; set; }
            public string team { get; set; }
            public string fy { get; set; }
            public string OCT_CNT { get; set; }
            public string OCT_REL { get; set; }
            public string OCT_WRKLD { get; set; }
            public string NOV_CNT { get; set; }
            public string NOV_REL { get; set; }
            public string NOV_WRKLD { get; set; }
            public string DEC_CNT { get; set; }
            public string DEC_REL { get; set; }
            public string DEC_WRKLD { get; set; }
            public string JAN_CNT { get; set; }
            public string JAN_REL { get; set; }
            public string JAN_WRKLD { get; set; }
            public string FEB_CNT { get; set; }
            public string FEB_REL { get; set; }
            public string FEB_WRKLD { get; set; }      
            public string MAR_CNT { get; set; }
            public string MAR_REL { get; set; }
            public string MAR_WRKLD { get; set; }  
            public string APR_CNT { get; set; }
            public string APR_REL { get; set; }
            public string APR_WRKLD { get; set; }  
            public string MAY_CNT { get; set; }
            public string MAY_REL { get; set; }
            public string MAY_WRKLD { get; set; }  
            public string JUN_CNT { get; set; }
            public string JUN_REL { get; set; }
            public string JUN_WRKLD { get; set; }  
            public string JUL_CNT { get; set; }
            public string JUL_REL { get; set; }
            public string JUL_WRKLD { get; set; }  
            public string AUG_CNT { get; set; }
            public string AUG_REL { get; set; }
            public string AUG_WRKLD { get; set; }  
            public string SEP_CNT { get; set; }
            public string SEP_REL { get; set; }
            public string SEP_WRKLD { get; set; }       
            public string TOTAL_CNT { get; set; }
            public string TOTAL_REL { get; set; }
            public string TOTAL_WRKLD { get; set; }
        }

        public static List<PMATWorkReports> LoadReports(string ic, string userid)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("sp_web_admin_gpmat_workload", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ic", System.Data.SqlDbType.VarChar).Value = ic;
            cmd.Parameters.Add("@operator", System.Data.SqlDbType.VarChar).Value = userid;
            conn.Open();

            var Reports = new List<PMATWorkReports>();
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Reports.Add(new PMATWorkReports
                {
                    specialist_name = rdr["specialist_name"]?.ToString(),
                    specialist_code = rdr["specialist_code"]?.ToString(),
                    branch = rdr["branch"]?.ToString(),
                    team = rdr["team"]?.ToString(),                  
                    fy= rdr["fy"]?.ToString(),
                    OCT_CNT= rdr["OCT_CNT"]?.ToString(),
                    OCT_REL= rdr["OCT_REL"]?.ToString(),
                    OCT_WRKLD= rdr["OCT_WRKLD"]?.ToString(),
                    NOV_CNT = rdr["NOV_CNT"]?.ToString(),
                    NOV_REL = rdr["NOV_REL"]?.ToString(),
                    NOV_WRKLD = rdr["NOV_WRKLD"]?.ToString(),
                    DEC_CNT = rdr["DEC_CNT"]?.ToString(),
                    DEC_REL = rdr["DEC_REL"]?.ToString(),
                    DEC_WRKLD = rdr["DEC_WRKLD"]?.ToString(),
                    JAN_CNT = rdr["JAN_CNT"]?.ToString(),
                    JAN_REL = rdr["JAN_REL"]?.ToString(),
                    JAN_WRKLD = rdr["JAN_WRKLD"]?.ToString(),
                    FEB_CNT = rdr["FEB_CNT"]?.ToString(),
                    FEB_REL = rdr["FEB_REL"]?.ToString(),
                    FEB_WRKLD = rdr["FEB_WRKLD"]?.ToString(),
                    MAR_CNT = rdr["MAR_CNT"]?.ToString(),
                    MAR_REL = rdr["MAR_REL"]?.ToString(),
                    MAR_WRKLD = rdr["MAR_WRKLD"]?.ToString(),
                    APR_CNT = rdr["APR_CNT"]?.ToString(),
                    APR_REL = rdr["APR_REL"]?.ToString(),
                    APR_WRKLD = rdr["APR_WRKLD"]?.ToString(),
                    MAY_CNT = rdr["MAY_CNT"]?.ToString(),
                    MAY_REL = rdr["MAY_REL"]?.ToString(),
                    MAY_WRKLD = rdr["MAY_WRKLD"]?.ToString(),
                    JUN_CNT = rdr["JUN_CNT"]?.ToString(),
                    JUN_REL = rdr["JUN_REL"]?.ToString(),
                    JUN_WRKLD = rdr["JUN_WRKLD"]?.ToString(),
                    JUL_CNT = rdr["JUL_CNT"]?.ToString(),
                    JUL_REL = rdr["JUL_REL"]?.ToString(),
                    JUL_WRKLD = rdr["JUL_WRKLD"]?.ToString(),
                    AUG_CNT = rdr["AUG_CNT"]?.ToString(),
                    AUG_REL = rdr["AUG_REL"]?.ToString(),
                    AUG_WRKLD = rdr["AUG_WRKLD"]?.ToString(),
                    SEP_CNT = rdr["SEP_CNT"]?.ToString(),
                    SEP_REL = rdr["SEP_REL"]?.ToString(),
                    SEP_WRKLD = rdr["SEP_WRKLD"]?.ToString(),
                    TOTAL_CNT = rdr["TOTAL_CNT"]?.ToString(),
                    TOTAL_REL = rdr["TOTAL_REL"]?.ToString(),
                    TOTAL_WRKLD = rdr["TOTL_WRKLD"]?.ToString()
                });
            }
            rdr.Close();
            conn.Close();

            return Reports;
        }
    }
}