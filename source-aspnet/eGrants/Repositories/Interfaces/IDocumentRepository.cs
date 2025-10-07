using eGrants.Models;

namespace eGrants.Repositories.Interfaces
{
    public interface IDocumentRepository
    {
        List<doclayer> LoadDocs(int aApplId, string aSearchType, string aCategoryList, string aIc, string aUserId);
    }
}
