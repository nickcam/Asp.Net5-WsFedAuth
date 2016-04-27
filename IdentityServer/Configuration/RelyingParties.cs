using System.Collections.Generic;

using System.Security.Claims;
using IdentityModel.Constants;
using IdentityServer3.WsFederation.Models;

namespace IdentityServer.Configuration
{
    public static class RelyingParties
    {
        public static IEnumerable<RelyingParty> Get()
        {
            return new List<RelyingParty> {
            new RelyingParty {
                Realm = "https://localhost:44346/",
                Name = "WsFedAuth.Web",
                Enabled = true,
                ReplyUrl = "https://localhost:44346/Home/Authenticated/",
                TokenType = TokenTypes.Saml2TokenProfile11,
                ClaimMappings =
                    new Dictionary<string, string> {
                        { "sub", ClaimTypes.NameIdentifier },
                        { "name", ClaimTypes.Name },
                        { "given_name", ClaimTypes.GivenName },
                        { "family_name", ClaimTypes.Surname },
                        { "email", ClaimTypes.Email },
                        { "upn", ClaimTypes.Upn }
                    }
            }
        };
        }
    }
}