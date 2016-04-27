using Microsoft.AspNet.Authentication.Cookies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WsFedAuth.Web.Middleware.WsFedAuthentication
{
    public class WsFedAuthenticationOptions : CookieAuthenticationOptions
    {
        public string Realm { get; set; }
        public string SigningCertThumbprint { get; set; }
        public string EncryptionCertStoreName { get; set; }
        public bool IsPersistent { get; set; }

        public string IdPEndpoint { get; set; }

    }
}
