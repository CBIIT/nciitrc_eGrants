using eGrants.Models;
using eGrants.Repositories.Interfaces;
using eGrants.Services.Interfaces;

namespace eGrants.Services
{
    // Defines a service class that implements the IeGrantService interface
    public class eGrantsService : IeGrantsService
    {
        // Dependency injection of a product repository to access data
        private readonly IeGrantsRepository _eGrantRepository;

        // Constructor that initializes the repository via dependency injection
        public eGrantsService(IeGrantsRepository eGrantRepository)
        {
            _eGrantRepository = eGrantRepository;
        }

        // Asynchronously retrieves a list of eGrants from the repository
        public async Task<List<eGrantsSearchResults>> GetEgrantsByStrAsync(string aSearchString, int aGrantId, string aPackage, int aApplId, int aCurrentPage, string aBrowser, string aIC, string aOperator)
        {
            // Placeholder for potential business logic before fetching data
            return await _eGrantRepository.GetEgrantsByStrAsync(aSearchString, aGrantId, aPackage, aApplId, aCurrentPage, aBrowser, aIC, aOperator);
        }
    }
}
