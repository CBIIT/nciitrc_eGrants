using eGrants.DTOs;
using eGrants.Models;
using eGrants.Repositories.Interfaces;
using eGrants.Services.Interfaces;

namespace eGrants.Services
{
    public class InstitutionalFilesService : IInstitutionalFilesService
    {

        private readonly IInstitutionalFilesRepository _institutionalFilesRepository;

        public InstitutionalFilesService(IInstitutionalFilesRepository institutionalFilesRepository)
        {
            _institutionalFilesRepository = institutionalFilesRepository;
        }

        public async Task<InstFileFindOrgDTO> FindOrg(int orgId, string orgName = "")
        {
            return await _institutionalFilesRepository.FindOrg(orgId, orgName);
        }

        public async Task<List<InsitutionalOrgNameIndex>> LoadOrgNameCharacterIndices()
        {
            return await _institutionalFilesRepository.LoadOrgNameCharacterIndices();
        }

        public async Task<List<InstFileLoadOrgDocListDTO>> LoadOrgDocList(int orgId)
        {
            return await _institutionalFilesRepository.LoadOrgDocList(orgId);
        }
    }
}
