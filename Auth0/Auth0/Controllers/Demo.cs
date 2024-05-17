using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Auth0.Controllers
{
    public class Demo : Controller
    {

        public async Task Login(string returnUrl = "/")
        {
            //Challenge metodu, kullanıcıyı belirtilen kimlik sağlayıcısına yönlendirir.
            await HttpContext.ChallengeAsync("Auth0", new AuthenticationProperties() { RedirectUri = returnUrl});

        }

        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Auth0", new AuthenticationProperties()
            {
                RedirectUri = Url.Action("Index", "Home")
            });
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
