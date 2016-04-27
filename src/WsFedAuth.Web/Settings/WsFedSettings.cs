using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WsFedAuth.Web.Settings
{
    public class WsFedSettings
    {
        public string AuthenticationScheme { get; set; }
        public string ClaimsIssuer { get; set; }
        public string IdPEndpoint { get; set; }
        public string Realm { get; set; }
        public string SigningCertThumbprint { get; set; }
        public string EncryptionCertStoreName { get; set; }
        public string CookieName { get; set; }
        public string AccessDeniedPath { get; set; }
        public string FederatedLogoutPath { get; set; }
        public int LoginTimeoutMinutes { get; set; }
        public bool IsPersistent { get; set; }
        public bool SlidingExpiration { get; set; }
        
    }
}
