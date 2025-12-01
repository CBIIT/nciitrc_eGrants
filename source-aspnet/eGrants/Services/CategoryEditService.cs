using System.Data;

using eGrants.DAL;
using eGrants.Models;
using eGrants.Repositories.Interfaces;
using eGrants.Services.Interfaces;
using eGrants.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace eGrants.Services
{
    public class CategoryEditService : ICategoryEditService
    {
        // Dependency injection of a product repository to access data
        private readonly ISessionInfoService _sessionInfoService;
        private readonly ICommonRepository _commonRepository;
        private readonly AppDbContext _context;

        // Constructor that initializes the repository via dependency injection
        public CategoryEditService(ISessionInfoService sessionInfoService, ICommonRepository commonRepository, 
            AppDbContext context)
        {
            _sessionInfoService = sessionInfoService;
            _commonRepository = commonRepository;
            _context = context;
        }

        public List<Categories> LoadCommonCategories(string ic)
        {
            using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
            {
                var cmd = new SqlCommand(
                    "select distinct ct.category_id, category_name from categories ct, categories_ic ci where ct.category_id = ci.category_id and ic <> @ic "
                  + "Union select distinct ct.category_id, category_name from categories ct, categories_ic ci where ct.category_id = ci.category_id and ic = @ic and removed_date is not null order by category_name",
                    conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                conn.Open();

                var CommonCategories = new List<Categories>();
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    CommonCategories.Add(
                        new Categories
                        {
                            category_id = Int32.Parse(rdr["category_id"].ToString()),
                            category_name = rdr["category_name"]?.ToString()
                        });

                rdr.Close();
                conn.Close();

                return CommonCategories;
            }
            
        }

        public List<Categories> LoadLocalCategories(string ic)
        {
            using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
            {
                var cmd = new SqlCommand("select distinct category_id, category_name from vw_categories where ic=@ic order by category_name", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@ic", SqlDbType.VarChar).Value = ic;
                conn.Open();

                var LocalCategories = new List<Categories>();
                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    LocalCategories.Add(
                        new Categories
                        {
                            category_id = Int32.Parse(rdr["category_id"].ToString()),
                            category_name = rdr["category_name"]?.ToString()
                        });

                rdr.Close();
                conn.Close();

                return LocalCategories;
            }
                
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
        public string run_db(string act, int category_id, string category_name, string ic, string userid)
        {
            using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
            {
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
}

