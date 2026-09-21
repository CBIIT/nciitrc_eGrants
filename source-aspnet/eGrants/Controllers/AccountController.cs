namespace eGrants.Controllers
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Identity.Web;

    public class AccountController : Controller
    {
        // App-only sign-out: clears the local application auth cookie and the
        // server-side session, but does NOT perform a federated (Entra ID)
        // sign-out. The shared Microsoft SSO session is left intact, so other
        // apps are unaffected. When the user returns, Entra may silently
        // re-authenticate them via SSO.
        [AllowAnonymous]
        [Route("Account/SignOutLocal")]
        public async Task<IActionResult> SignOutLocal()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Redirect("/");
        }

        // Forced re-authentication after an inactivity timeout.
        //
        // Clears the local application cookie/session, then issues a standard
        // OIDC challenge. The global OpenIdConnectOptions.Prompt = "login" (see
        // Program.cs) already forces Entra to interactively re-prompt for
        // credentials on the resulting authorize request.
        //
        // [AllowAnonymous] is REQUIRED: the global authorization FallbackPolicy
        // requires an authenticated user on every endpoint. Without it, a user
        // whose cookie has already expired hits this endpoint unauthenticated,
        // gets challenged once by the fallback policy, and then this action
        // issues a second challenge — resulting in being prompted to sign in
        // twice. AllowAnonymous lets the action run its single explicit challenge.
        [AllowAnonymous]
        [Route("Account/ReAuthenticate")]
        public async Task<IActionResult> ReAuthenticate()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var properties = new OpenIdConnectChallengeProperties
            {
                RedirectUri = "/"
            };

            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
