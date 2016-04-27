using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer3.Core;
using IdentityServer3.Core.Services.InMemory;

namespace IdentityServer.Configuration
{
    public static class Users
    {
        public static List<InMemoryUser> Get()
        {
            return new List<InMemoryUser>
            {
                new InMemoryUser
                {
                    Username = "nick",
                    Password = "password",
                    Subject = "1",
                    Claims = new List<Claim>
                    {
                        new Claim(Constants.ClaimTypes.Name, "nick"),
                        new Claim(Constants.ClaimTypes.GivenName, "Nick"),
                        new Claim(Constants.ClaimTypes.FamilyName, "Cameron"),
                        new Claim(Constants.ClaimTypes.Email, "nick@nick.com"),
                        new Claim(Constants.ClaimTypes.Role, "admin")
                    }
                }
            };
        }
    }
}