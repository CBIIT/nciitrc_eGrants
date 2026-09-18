namespace eGrants.Controllers
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Mvc;

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
    }
}
