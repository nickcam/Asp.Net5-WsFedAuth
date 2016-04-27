
using System.Collections.Generic;

using IdentityServer;
using IdentityServer3.Core.Configuration;
using IdentityServer3.WsFederation.Configuration;
using IdentityServer3.WsFederation.Models;
using IdentityServer3.WsFederation.Services;

using Owin;
using IdentityServer.Configuration;
using Microsoft.Owin;

[assembly: OwinStartup("WsFederation", typeof(Startup))]
namespace IdentityServer
{
    /// <summary>
    /// This whole project is just a dummy WS-Fed IdP. Basically an exact copy of this project - https://github.com/scottbrady91/IdentityServer3-Example
    /// </summary>
    public sealed class Startup
    {
       
        public void Configuration(IAppBuilder app)
        {
            app.Map(
                "/core",
                coreApp =>
                {
                    coreApp.UseIdentityServer(
                    new IdentityServerOptions
                    {
                        SiteName = "Standalone Identity Server",
                        SigningCertificate = Cert.Load(),
                        Factory =
                                new IdentityServerServiceFactory().UseInMemoryClients(Clients.Get())
                                .UseInMemoryScopes(Scopes.Get())
                                .UseInMemoryUsers(Users.Get()),
                        RequireSsl = true,
                        PluginConfiguration = ConfigureWsFederation
                    });
                });
        }

        private void ConfigureWsFederation(IAppBuilder pluginApp, IdentityServerOptions options)
        {
            var factory = new WsFederationServiceFactory(options.Factory);
            factory.Register(new Registration<IEnumerable<RelyingParty>>(RelyingParties.Get()));
            factory.RelyingPartyService = new Registration<IRelyingPartyService>(typeof(InMemoryRelyingPartyService));
            pluginApp.UseWsFederationPlugin(new WsFederationPluginOptions { IdentityServerOptions = options, Factory = factory });
        }
    }
}