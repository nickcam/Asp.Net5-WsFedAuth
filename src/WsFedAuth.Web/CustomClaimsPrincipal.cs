using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WsFedAuth.Web
{


    /// <summary>
    /// A Custom Claims Principal for injection - could add other properties like App specific properties from a DB or something if you wanted.
    /// This one doesn't add anything though.
    /// </summary>
    public class CustomClaimsPrincipal : ClaimsPrincipal
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomClaimsPrincipal(IHttpContextAccessor httpContextAccessor) : base()
        {
            _httpContextAccessor = httpContextAccessor;
            this.AddIdentity(_httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity);
        }


    }
}
