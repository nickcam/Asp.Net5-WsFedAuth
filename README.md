# Asp.Net5-WsFedAuth
Example / sandpit of custom middleware for WsFed Authentication

Apparentely Asp.Net 5 won't have Ws-Fed middleware in the first release.
See this issue: https://github.com/aspnet/Security/issues/605

Have a new project where I need to integrate with a custom STS that uses Ws-Fed, so this is a crack at building some custom middleware to handle it. Thought about creating an RP-STS or bridge to plug the gap between the STS and Asp.Net5 app, but thought it would be nicer to just have some middleware to do it. Not sure if it's the right way to go or if the implementation is any good yet or not.

The basic idea of it is just to inherit from the Cookie Authenitcation middleware where possible and then add some redirects to the WsFed endpoint. So the Asp.Net5 app is essentially authenticated using cookie auth middleware, but there's some extra handling where needed that uses some extra options to configure the app to hit the WsFed endpoint, before creating any local auth cookies for the app.

### Caveats:
Only good for .Net 4.5.1 - won't work using DNXCore. It uses some classes from System.IdentityModel.Services. Not an issue if you're deploying on IIS or Azure anyway.

### The example solution
Contains two projects
- The Asp.Net 5 web app - WsFedAuth.Web
  - Built using 1.0.0-rc1-update1.
  - The custom middleware is at /Middleware/WsFedAuthentication/.
  - Just a few basic pages, one of them is authenticated.
  - Each page will display if you're logged in or not.
   
- IdentityServer
  - This is just a test WsFed IdP to use for the example. It's a super basic IdentityServer 3 setup, taken almost verbatim from this repository: https://github.com/scottbrady91/IdentityServer3-Example.
  - Had to set up IdentityServer project to run under Local IIS - wouldn't work for me using IIS Express, so you may have to create the virtual directory for IdentityServer.
  
There's one user configured - 
un: nick
pw: password

### Issues:
Federated Logout is a bit hacky. Wasn't able to redirect within the WsFedAuthenticationHandler during logout, so just set a header value on the response that the controller action checks for and redirects to if needed. Must be a way to redirect to the WsFed endpoint in the handler instead of having to hit the controller. Redirecting works during the SignIn just not SignOut??

Also during the Federated Log out, IdentityServer will log you out after asking for your permission, but won't redirect back to the reply page. The custom STS I've tested this on does redirect after federated logout, so this is just WsFed endpoint dependant.

---------------------------------
The process is working for my Custom STS (and the incldued example IdentityServer). Still not 100% sure if this the way to go or not, so if there's better options please let me (and anyone else who sees this) know.


