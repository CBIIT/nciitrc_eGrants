using eGrants.DTOs;
using eGrants.Models;

namespace eGrants.Repositories.Interfaces
{
    public interface IInstitutionalFilesRepository
    {
        Task<InstFileFindOrgDTO> FindOrg(int orgId, string orgName = "");

        Task<List<InsitutionalOrgNameIndex>> LoadOrgNameCharacterIndices();

        Task<List<InstFileLoadOrgDocListDTO>> LoadOrgDocList(int orgId);
    }
}
