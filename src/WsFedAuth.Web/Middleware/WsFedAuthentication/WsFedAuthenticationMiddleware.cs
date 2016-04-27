using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.IdentityModel.Services;
using System.Collections.Specialized;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.IdentityModel.Services.Tokens;
using System.Security.Claims;

using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders;
using Microsoft.Extensions.OptionsModel;

namespace WsFedAuth.Web.Middleware.WsFedAuthentication
{

    public class WsFedAuthenticationMiddleware : AuthenticationMiddleware<WsFedAuthenticationOptions>
    {

        public WsFedAuthenticationMiddleware(
            RequestDelegate next,
            IDataProtectionProvider dataProtectionProvider,
            ILoggerFactory loggerFactory,
            IUrlEncoder urlEncoder,
            WsFedAuthenticationOptions options
            ) : base(next, options, loggerFactory, urlEncoder)
        {
            
            if (dataProtectionProvider == null)
            {
                throw new ArgumentNullException(nameof(dataProtectionProvider));
            }

            if (Options.Events == null)
            {
                Options.Events = new CookieAuthenticationEvents();
            }

            if (String.IsNullOrEmpty(Options.CookieName))
            {
                Options.CookieName = CookieAuthenticationDefaults.CookiePrefix + Options.AuthenticationScheme;
            }

            if (Options.TicketDataFormat == null)
            {
                var provider = Options.DataProtectionProvider ?? dataProtectionProvider;
                var dataProtector = provider.CreateProtector(typeof(CookieAuthenticationMiddleware).FullName, Options.AuthenticationScheme, "v2");
                Options.TicketDataFormat = new TicketDataFormat(dataProtector);
            }

            if (Options.CookieManager == null)
            {
                Options.CookieManager = new ChunkingCookieManager(urlEncoder);
            }


            if (!Options.AccessDeniedPath.HasValue)
            {
                Options.AccessDeniedPath = CookieAuthenticationDefaults.AccessDeniedPath;
            }

        }

        protected override AuthenticationHandler<WsFedAuthenticationOptions> CreateHandler()
        {
            return new WsFedAuthenticationHandler();
        }

    }

   
}
