using Microsoft.AspNet.Authentication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Features.Authentication;
using System.IdentityModel.Services;
using Microsoft.AspNet.Http;
using System.Collections.Specialized;
using System.Security.Claims;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Microsoft.AspNet.Authentication.Cookies;

namespace WsFedAuth.Web.Middleware.WsFedAuthentication
{

    /// <summary>
    /// Custom Authentication Handler. Overrides a custom Cookie Handler that is basically wholesale copy of CookieAuthenticationHandler. We let CookieHandler deal with the nitty
    /// gritty of the cookie details for this app. Just value adding by creating and parsing WS-FED requests/responses where appropriate. 
    /// </summary>
    internal class WsFedAuthenticationHandler : WsFedCookieAuthenticationHandler
    {
       
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return await base.HandleAuthenticateAsync();
        }


        protected override async Task HandleSignInAsync(SignInContext context)
        {         
            WsFedSignInContext signInContext = context as WsFedSignInContext;
            ClaimsPrincipal principal = GetClaimsPrincipal(signInContext.SignInMessage);

            if (principal != null)
            {
                WsFedSignInContext newContext = new WsFedSignInContext(context.AuthenticationScheme, principal, context.Properties, null, signInContext.ReturnUrl);
                await base.HandleSignInAsync(newContext);
                return;
            }

            //Couldn't get a principal even though we've said sign in, so send to forbidden - could be the wrong STS environment, or incorrect certificate config, or some other accidental or nefarious reason for this.
            ChallengeContext cc = new ChallengeContext(Options.AuthenticationScheme);
            await base.HandleForbiddenAsync(cc);
        }


        protected override Task HandleSignOutAsync(SignOutContext signOutContext)
        {
            //do the default cookie sign out to kill the apps local cookie.
            var result = base.HandleSignOutAsync(signOutContext);
            
            //create the Fed Sign Out url from a SignOutRequestMessage
            var logOutPath = Options.LogoutPath.HasValue ? Options.LogoutPath : new PathString("/");
            string replyUrl = $"{Request.Scheme}://{Request.Host}{logOutPath}";
            SignOutRequestMessage req = new SignOutRequestMessage(new Uri(Options.IdPEndpoint));
            req.Parameters.Add("wtrealm", Options.Realm);
            req.Parameters.Add("wreply", replyUrl);
            var signOutUrl = req.WriteQueryString();
       
            //Add a header to the response containing the fed sign out url. Did this as Redirecting from here in the pipeline doesn't seem to work.
            //Bit of a Hack - this header can be read later if a Fed Sign Out is required.
            Response.Headers.Add("fedSignOutUrl", "https://localhost/IdentityServer/core/wsfed/?wa=wsignout1.0&wtrealm=https%3a%2f%2flocalhost%3a44346%2f&wreply=https%3a%2f%2flocalhost%3a44346%2f");

            return result;
        }

       
        protected override Task FinishResponseAsync()
        {
            return base.FinishResponseAsync();
        }

        protected override Task<bool> HandleForbiddenAsync(ChallengeContext context)
        {
            return base.HandleForbiddenAsync(context);
        }

        protected override async Task<bool> HandleUnauthorizedAsync(ChallengeContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //create a return url that is the current requests full url
            var returnUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}";
            
            //if user not authenticated but the form post contains a SignInResponseMessage - so they've come back from the IdP after signing in - handle it to sign the user in to this app.
            var signInResponse = GetSignInResponseMessage();
            if (signInResponse != null)
            {
                Dictionary<string, string> props = new Dictionary<string, string>();

                //Add the persistent option to the props for the cookie handler here if it's set to true in the Options object. This is to have IsPersistent available as a higher level option instead of
                //having to pass to SignInAsync as an option.
                if (Options.IsPersistent)
                {
                    props.Add(".persistent", "");
                }

                WsFedSignInContext c = new WsFedSignInContext(Options.AuthenticationScheme, Context.User, props, signInResponse, returnUrl);
                await this.SignInAsync(c);
                return true;
            }

            //User is not authenticated, so create SignInRequest message to send to IdP endpoint, and redirect there.
            SignInRequestMessage req = new SignInRequestMessage(new Uri(Options.IdPEndpoint), Options.Realm, returnUrl);
            var signInUrl = req.RequestUrl;
            var redirectContext = new CookieRedirectContext(Context, Options, signInUrl);            
            await Options.Events.RedirectToLogin(redirectContext);
            return true;           
        }

        private SignInResponseMessage GetSignInResponseMessage()
        {
            if (Context.Request.HasFormContentType)
            {
                //convert form into NameValueCollection for SignInResponseMessage checking.
                var nvc = new NameValueCollection();
                foreach (var fv in Context.Request.Form)
                {
                    nvc.Add(fv.Key, fv.Value);
                }

                //check if this is a sign in response
                SignInResponseMessage signInResponse = SignInResponseMessage.CreateFromNameValueCollection(new Uri(Options.Realm), nvc) as SignInResponseMessage;
                return signInResponse;
            }

            return null;
        }


        public override Task<bool> HandleRequestAsync()
        {
            return base.HandleRequestAsync();
        }


        private static bool IsAjaxRequest(HttpRequest request)
        {
            return string.Equals(request.Query["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal) ||
                string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal);
        }

        private ClaimsPrincipal GetClaimsPrincipal(SignInResponseMessage signInResponse)
        {
            try
            {
                //configure the certificate and some service token handler configuration properties (these basically match the web.config settings for MVC 4/5 app).
                SecurityTokenHandlerConfiguration config = new SecurityTokenHandlerConfiguration();
                config.AudienceRestriction.AllowedAudienceUris.Add(new Uri(Options.Realm));
                config.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None; //we have dodgy certs in dev

                ConfigurationBasedIssuerNameRegistry inr = new ConfigurationBasedIssuerNameRegistry();
                inr.AddTrustedIssuer(Options.SigningCertThumbprint, Options.ClaimsIssuer);
                config.IssuerNameRegistry = inr;
                config.CertificateValidator = System.IdentityModel.Selectors.X509CertificateValidator.None; //we have dodgy certs in dev

                //Load up an XmlDocument with the result. Have to use XmlDocument so we can generate a valid reader unfortunately.
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(signInResponse.Result);

                //Add the namespaces and search for Assertion or EncryptedAssertion
                XmlNamespaceManager nsMan = new XmlNamespaceManager(xmlDoc.NameTable);
                nsMan.AddNamespace("trust", "http://docs.oasis-open.org/ws-sx/ws-trust/200512");
                nsMan.AddNamespace("saml2", "urn:oasis:names:tc:SAML:2.0:assertion");
                var parentNodes = "trust:RequestSecurityTokenResponseCollection/trust:RequestSecurityTokenResponse/trust:RequestedSecurityToken/";
                var assertionNode = xmlDoc.SelectSingleNode(parentNodes + "saml2:EncryptedAssertion", nsMan);
                if(assertionNode == null)
                {
                    assertionNode = xmlDoc.SelectSingleNode(parentNodes + "saml2:Assertion", nsMan);
                }
                else
                {
                    //this is an encrypted response so add a ServiceTokenResolver of X509CertificateStoreTokenResolver so the assertion can be decrypted. Hard codes LocalMachine - could be configured as well.
                    config.ServiceTokenResolver = new X509CertificateStoreTokenResolver(Options.EncryptionCertStoreName, StoreLocation.LocalMachine);
                }

                if(assertionNode == null)
                {
                    throw new Exception("No assertion element found in Response.");
                }

                using (var reader = new XmlNodeReader(assertionNode))
                {
                    //Get the token and convert it to a Claims Principal for return
                    SecurityTokenHandlerCollection collection = SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection(config);
                    var securityToken = collection.ReadToken(reader);
                    var claimsIdentities = collection.ValidateToken(securityToken);

                    ClaimsPrincipal principal = new ClaimsPrincipal(claimsIdentities);
                    return principal;
                }

            }
            catch (Exception ex)
            {
                //TODO: Add some logging
                var err = ex;
            }
           

            return null;
        }


    }
}
