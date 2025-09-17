using eGrants.Models;
using eGrants.Services.Interfaces;

namespace eGrants.Services
{
    public class SessionInfoService : ISessionInfoService
    {
        public SessionInfo GetSessionInfo(HttpContext httpContext)
        {
            var session = httpContext.Session;

            return new SessionInfo
            {
                Ic = session.GetString("ic"),
                Browser = session.GetString("browser"),
                UserId = session.GetString("userid")
            };
        }
    }
}
