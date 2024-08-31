using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mvc.Models;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

namespace mvc.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Secret()
        {

            var accessToken = HttpContext.GetTokenAsync("access_token").Result;
            var idToken = HttpContext.GetTokenAsync("id_token").Result;

            var claims = User.Claims;

            var _accessToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var _idToken = new JwtSecurityTokenHandler().ReadJwtToken(idToken);

            return View();
        }
        public IActionResult Logout()
        {
            return SignOut("Cookie", "oidc");
        }

       
    }
}
