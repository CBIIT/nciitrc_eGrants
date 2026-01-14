using System.Text;

using eGrants.Models;
using eGrants.Services.Interfaces;

namespace eGrants.Services
{
    public class SessionInfoService : ISessionInfoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionInfoService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public SessionInfo GetSessionInfo(ISession session)
        {
            // Get browser cookies from the current HTTP request
            var browserCookies = _httpContextAccessor.HttpContext?.Request.Headers["Cookie"].ToString() ?? string.Empty;

            return new SessionInfo
            {
                Ic = session.TryGetValue("ic", out var icBytes) && icBytes != null ? System.Text.Encoding.UTF8.GetString(icBytes) : "",
                Browser = session.TryGetValue("browser", out var browserBytes) && browserBytes != null ? System.Text.Encoding.UTF8.GetString(browserBytes) : "",
                UserId = session.TryGetValue("userid", out var userBytes) && userBytes != null ? System.Text.Encoding.UTF8.GetString(userBytes) : "",
                WebGrantUrl = session.TryGetValue("WebGrantUrl", out var webGrantUrlBytes) && webGrantUrlBytes != null ? System.Text.Encoding.UTF8.GetString(webGrantUrlBytes) : "",
                ImageServerUrl = session.TryGetValue("ImageServerUrl", out var imageServerUrlBytes) && imageServerUrlBytes != null ? System.Text.Encoding.UTF8.GetString(imageServerUrlBytes) : "",
                EgrantsDocNewRelativePath = session.TryGetValue("EgrantsDocNewRelativePath", out var NewRelativePathBytes) && NewRelativePathBytes != null ? System.Text.Encoding.UTF8.GetString(NewRelativePathBytes) : "",
                EgrantsDocModifyRelativePath = session.TryGetValue("EgrantsDocModifyRelativePath", out var ModifyrelativePathBytes) && ModifyrelativePathBytes != null ? System.Text.Encoding.UTF8.GetString(ModifyrelativePathBytes) : "",
                EgrantsDocEmail = session.TryGetValue("EgrantsDocEmail", out var EgrantsDocEmailBytes) && EgrantsDocEmailBytes != null ? System.Text.Encoding.UTF8.GetString(EgrantsDocEmailBytes) : "",
                CertPath = session.TryGetValue("CertPath", out var certPathBytes) && certPathBytes != null ? System.Text.Encoding.UTF8.GetString(certPathBytes) : "",
                CertPass = session.TryGetValue("CertPass", out var certPassBytes) && certPassBytes != null ? System.Text.Encoding.UTF8.GetString(certPassBytes) : "",
                EraUrlBase = session.TryGetValue("EraUrlBase", out var eraUrlBaseBytes) && eraUrlBaseBytes != null ? System.Text.Encoding.UTF8.GetString(eraUrlBaseBytes) : "",
                BrowserCookies = browserCookies
            };
        }
    }
}