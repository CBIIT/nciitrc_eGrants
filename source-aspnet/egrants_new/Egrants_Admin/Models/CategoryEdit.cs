#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  CategoryEdit.cs
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

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

using egrants_new.Egrants.Models;

#endregion

namespace egrants_new.Egrants_Admin.Models
{
    /// <summary>
    /// The category edit.
    /// </summary>
    public class CategoryEdit
    {
        /// <summary>
        /// The load common categroies.
        /// </summary>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<EgrantsCategories> LoadCommonCategroies(string ic)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);

            var cmd = new SqlCommand(
                "select distinct ct.category_id, category_name from categories ct, categories_ic ci where ct.category_id = ci.category_id and ic <> @ic "
              + "Union select distinct ct.category_id, category_name from categories ct, categories_ic ci where ct.category_id = ci.category_id and ic = @ic and removed_date is not null order by category_name",
                conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            conn.Open();

            var CommonCategroies = new List<EgrantsCategories>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                CommonCategroies.Add(
                    new EgrantsCategories
                        {
                            category_id = rdr["category_id"]?.ToString(), category_name = rdr["category_name"]?.ToString()
                        });

            rdr.Close();
            conn.Close();

            return CommonCategroies;
        }

        /// <summary>
        /// The load local categroies.
        /// </summary>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<EgrantsCategories> LoadLocalCategroies(string ic)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("select distinct category_id, category_name from vw_categories where ic=@ic order by category_name", conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
            conn.Open();

            var LocalCategroies = new List<EgrantsCategories>();
            var rdr = cmd.ExecuteReader();

            while (rdr.Read())
                LocalCategroies.Add(
                    new EgrantsCategories
                        {
                            category_id = rdr["category_id"]?.ToString(), category_name = rdr["category_name"]?.ToString()
                        });

            rdr.Close();
            conn.Close();

            return LocalCategroies;
        }

        /// <summary>
        /// The run_db.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="category_id">
        /// The category_id.
        /// </param>
        /// <param name="category_name">
        /// The category_name.
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
        public static string run_db(string act, int category_id, string category_name, string ic, string userid)
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["EgrantsDB"].ConnectionString);
            var cmd = new SqlCommand("dbo.sp_web_admin_category_edit", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@act", SqlDbType.VarChar).Value = act;
            cmd.Parameters.Add("@category_id", SqlDbType.Int).Value = category_id;
            cmd.Parameters.Add("@category_name", SqlDbType.VarChar).Value = category_name;
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
    }
}