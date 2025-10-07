using eGrants.Models;

namespace eGrants.Services.Interfaces
{
    public interface IDocumentService
    {
        public List<doclayer> LoadDocs(int applId, string searchType, string categoryList, string mode, ISession sessionInfo);
    }
}
