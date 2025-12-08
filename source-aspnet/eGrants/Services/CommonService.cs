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
            AppDbContext context = null)
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

        public List<Profiles> LoadProfiles()
        {
            var list = new List<Profiles>();

            using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
            {
                var cmd = new SqlCommand("select profile_id, [profile], admin_phs_org_code from profiles order by admin_phs_org_code", conn);
                cmd.CommandType = CommandType.Text;
                conn.Open();


                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(
                        new Profiles
                        {
                            ProfileId = rdr["profile_id"]?.ToString(),
                            Profile = rdr["profile"]?.ToString(),
                            AdminPhsOrgCode = rdr["admin_phs_org_code"]?.ToString()
                        });
                }
            }

            return list;
        }

        public List<Position> LoadPositions()
        {
            List<Position> positions = new List<Position>();

            using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
            {
                var cmd = new SqlCommand("select position_id, position_name from people_positions order by position_id", conn);
                cmd.CommandType = CommandType.Text;
                conn.Open();

                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    positions.Add(new Position { PositionId = rdr["position_id"].ToString(), PositionName = rdr["position_name"].ToString() });
                }
            }

            return positions;
        }

        public List<EgrantsUsers> LoadCoordinators()
        {
            var list = new List<EgrantsUsers>();

            using (var conn = new SqlConnection(_context.Database.GetConnectionString()))
            {
                var cmd = new SqlCommand("select person_id, person_name from vw_people where is_coordinator=1 order by person_name", conn);
                cmd.CommandType = CommandType.Text;
                conn.Open();

                var rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new EgrantsUsers { PersonId = rdr["person_id"]?.ToString(), person_name = rdr["person_name"]?.ToString() });
                }
            }

            return list;
        }
    }
}
