using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using Microsoft.Extensions.OptionsModel;
using WsFedAuth.Web.Settings;
using System.IdentityModel.Services;
using Microsoft.AspNet.Http;
using WsFedAuth.Web.Middleware.WsFedAuthentication;
using Microsoft.AspNet.Http.Authentication;
using System.Security.Claims;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WsFedAuth.Web.Controllers
{
    public class HomeController : Controller
    {
        private CustomClaimsPrincipal _currentPrincipal;
        private WsFedSettings _wsFedSettings;


        public HomeController(CustomClaimsPrincipal currentPrincipal, IOptions<WsFedSettings> wsFedSettings)
        {
            _currentPrincipal = currentPrincipal;
            _wsFedSettings = wsFedSettings.Value;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Unauthorized()
        {
            return View();
        }


        [Authorize()]
        public IActionResult Authenticated()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            if (_currentPrincipal.Identity.IsAuthenticated)
            {
                await HttpContext.Authentication.SignOutAsync(_wsFedSettings.AuthenticationScheme);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> FederatedLogout()
        {
            if (_currentPrincipal.Identity.IsAuthenticated)
            {
                await HttpContext.Authentication.SignOutAsync(_wsFedSettings.AuthenticationScheme);

                //get the Fed Sign out Url from the header set in the Handler and redirect there if it exists
                var fedSignOutUrl = HttpContext.Response.Headers["fedSignOutUrl"];
                if (!String.IsNullOrWhiteSpace(fedSignOutUrl))
                {
                    return new RedirectResult(fedSignOutUrl);
                }
            }

            return RedirectToAction("Index");
        }

       
    }
}
