namespace eGrants.Controllers
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Identity.Web;

    public class AccountController : Controller
    {
        // App-only sign-out: clears the local application auth cookie and the
        // server-side session, but does NOT perform a federated (Entra ID)
        // sign-out. The shared Microsoft SSO session is left intact, so other
        // apps are unaffected. When the user returns, Entra may silently
        // re-authenticate them via SSO.
        [Route("Account/SignOutLocal")]
        public async Task<IActionResult> SignOutLocal()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Redirect("/");
        }

        // Forced re-authentication after an inactivity timeout.
        //
        // Clears the local application cookie/session, then challenges Entra ID
        // with prompt=login. Unlike a plain sign-out (which allows a silent SSO
        // re-login), prompt=login instructs Entra to interactively re-prompt the
        // user for credentials, and any Conditional Access / MFA policies are
        // re-evaluated. This guarantees eGrants requires fresh authentication.
        [Route("Account/ReAuthenticate")]
        public async Task<IActionResult> ReAuthenticate()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var properties = new OpenIdConnectChallengeProperties
            {
                RedirectUri = "/",
                Prompt = "login"
            };

            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
