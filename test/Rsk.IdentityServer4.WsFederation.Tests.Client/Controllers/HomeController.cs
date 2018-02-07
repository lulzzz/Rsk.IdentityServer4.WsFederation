using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Rsk.IdentityServer4.WsFederation.Tests.Client.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Login()
        {
            return View("Index");
        }

        public IActionResult Logout()
        {
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme,
                WsFederationDefaults.AuthenticationScheme);
        }
    }
}
