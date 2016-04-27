using Microsoft.AspNet.Http.Features.Authentication;
using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WsFedAuth.Web.Middleware.WsFedAuthentication
{
    public class WsFedSignInContext : SignInContext
    {
        public SignInResponseMessage SignInMessage { get; private set; }
        public string ReturnUrl { get; private set; }

        public WsFedSignInContext(string authenticationScheme, ClaimsPrincipal principal, IDictionary<string, string> properties, SignInResponseMessage signInMessage, string returnUrl) : base(authenticationScheme, principal, properties)
        {
            SignInMessage = signInMessage;
            ReturnUrl = returnUrl;
        }
    }
}
