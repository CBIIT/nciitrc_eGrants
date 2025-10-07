using System.Text;

using eGrants.Models;
using eGrants.Services.Interfaces;

namespace eGrants.Services
{
    public class SessionInfoService : ISessionInfoService
    {
        public SessionInfo GetSessionInfo(ISession session)
        {
            return new SessionInfo
            {
                Ic = session.TryGetValue("ic", out var icBytes) && icBytes != null ? System.Text.Encoding.UTF8.GetString(icBytes) : "",
                Browser = session.TryGetValue("browser", out var browserBytes) && browserBytes != null ? System.Text.Encoding.UTF8.GetString(browserBytes) : "",
                UserId = session.TryGetValue("userid", out var userBytes) && userBytes != null ? System.Text.Encoding.UTF8.GetString(userBytes) : "",
            };
        }
    }
}
