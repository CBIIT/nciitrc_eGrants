using eGrants.Models;

namespace eGrants.Services.Interfaces
{
    public interface ISessionInfoService
    {
        SessionInfo GetSessionInfo(ISession sessionInfo);
    }
}
