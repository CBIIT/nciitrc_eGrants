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
                WebGrantUrl = session.TryGetValue("WebGrantUrl", out var webGrantUrlBytes) && webGrantUrlBytes != null ? System.Text.Encoding.UTF8.GetString(webGrantUrlBytes) : "",
                ImageServerUrl = session.TryGetValue("ImageServerUrl", out var imageServerUrlBytes) && imageServerUrlBytes != null ? System.Text.Encoding.UTF8.GetString(imageServerUrlBytes) : "",
                EgrantsDocNewRelativePath = session.TryGetValue("EgrantsDocNewRelativePath", out var NewRelativePathBytes) && NewRelativePathBytes != null ? System.Text.Encoding.UTF8.GetString(NewRelativePathBytes) : "",
                EgrantsDocModifyRelativePath = session.TryGetValue("EgrantsDocModifyRelativePath", out var ModifyrelativePathBytes) && ModifyrelativePathBytes != null ? System.Text.Encoding.UTF8.GetString(ModifyrelativePathBytes) : "",
                EgrantsDocEmail = session.TryGetValue("EgrantsDocEmail", out var EgrantsDocEmailBytes) && EgrantsDocEmailBytes != null ? System.Text.Encoding.UTF8.GetString(EgrantsDocEmailBytes) : ""
            };
        }
    }
}
