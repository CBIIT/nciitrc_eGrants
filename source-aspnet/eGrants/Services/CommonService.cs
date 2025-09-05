using eGrants.Models;
using eGrants.Repositories;
using eGrants.Repositories.Interfaces;
using eGrants.Services.Interfaces;

namespace eGrants.Services
{
    public class CommonService : ICommonService
    {
        private readonly ICommonRepository _commonRepository;

        public CommonService(ICommonRepository commonRepository)
        {
            _commonRepository = commonRepository;
        }

        // Asynchronously retrieves a list of administrative codes from the common repository
        public async Task<List<AdminCodes>> LoadAdminCodes()
        {
            // Implementation to load admin codes
            return await _commonRepository.LoadAdminCodes();
        }
    }
}
