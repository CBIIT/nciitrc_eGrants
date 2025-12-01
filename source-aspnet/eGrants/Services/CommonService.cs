using System.Data;

using eGrants.DAL;
using eGrants.Models;
using eGrants.Repositories;
using eGrants.Repositories.Interfaces;
using eGrants.Services.Interfaces;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace eGrants.Services
{
    public class CommonService : ICommonService
    {
        private readonly ICommonRepository _commonRepository;
        private readonly AppDbContext _context;

        public CommonService(ICommonRepository commonRepository, 
            AppDbContext context)
        {
            _commonRepository = commonRepository;
            _context = context;
        }

        // Asynchronously retrieves a list of administrative codes from the common repository
        public async Task<List<AdminCodes>> LoadAdminCodes()
        {
            // Implementation to load admin codes
            return await _commonRepository.LoadAdminCodes();
        }

        public List<CharacterIndex> LoadCharacterIndex()
        {
            var list = new List<CharacterIndex>();

            using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
            {
                var cmd = new SqlCommand(
                    "SELECT index_id, character_index, index_seq from dbo.character_index where index_id>1 order by index_seq",
                    conn);

                cmd.CommandType = CommandType.Text;
                conn.Open();


                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(
                        new CharacterIndex
                        {
                            index_id = rdr["index_id"]?.ToString(),
                            character_index = rdr["character_index"]?.ToString(),
                            index_seq = rdr["index_seq"]?.ToString()
                        });
                }

            }

            return list;
        }
    }
}
