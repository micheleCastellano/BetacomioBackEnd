using Azure.Core;
using Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Text;
using Main.Repository;
using Main.Models;
using UtilityLibrary;

namespace Main.Authentication
{
    public class AdminAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ICredentialRepository _credentialRepository;

        public AdminAuthenticationHandler(
            ICredentialRepository credentialRepository,
           IOptionsMonitor<AuthenticationSchemeOptions> options,
           ILoggerFactory logger,
           UrlEncoder encoder,
           ISystemClock clock
           ) : base(options, logger, encoder, clock)
        {
            _credentialRepository = credentialRepository;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {

            Response.Headers.Add("WWW-Authenticate", "Basic");

            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Autorizzazione mancante"));
            }

            var authorizationHeader = Request.Headers["Authorization"].ToString();

            if (BasicAuthenticationUtilities.GetUsernamePassword(authorizationHeader) == ("", ""))
                return Task.FromResult(AuthenticateResult.Fail("Authorization Code, not properly formatted"));


            (string authEmailAddress, string authPassword) = BasicAuthenticationUtilities.GetUsernamePassword(authorizationHeader);

            // CHANGE CONTROL BELOW TO CHECK ADMIN RIGHTS
            var result = _credentialRepository.CheckLoginBetacomio(authEmailAddress, authPassword);



            if (result == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("User e/o password errati !!!"));
            }

            Credential credential = result;

            if (!_credentialRepository.CheckIfAdmim(credential.EmailAddress))
                return Task.FromResult(AuthenticateResult.Fail("User is not admin"));

            var authenticatedUser = new AuthenticatedUser("BasicAuthentication", true, credential.EmailAddress);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(authenticatedUser));

            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));

        }
    }
}
