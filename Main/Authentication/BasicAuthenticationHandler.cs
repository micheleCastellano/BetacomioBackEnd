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

namespace Main.Authentication
{
    public class BasicAuthenticationHandler: AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ICredentialRepository _credentialRepository;

        public BasicAuthenticationHandler(
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
            var authoHeaderRegEx = new Regex("Basic (.*)");

            if (!authoHeaderRegEx.IsMatch(authorizationHeader))
            {
                return Task.FromResult(AuthenticateResult.Fail("Authorization Code, not properly formatted"));
            }

            var authBase64 = Encoding.UTF8.GetString(Convert.FromBase64String(authoHeaderRegEx.Replace(authorizationHeader, "$1")));
            var authSplit = authBase64.Split(Convert.ToChar(":"), 2);
            var authEmailAddress = authSplit[0];
            var authPassword = authSplit.Length > 1 ? authSplit[1] : throw new Exception("Unable to get Password");

            var result = _credentialRepository.CheckLoginBetacomio(authEmailAddress, authPassword);

            if (result==null)
            {
                return Task.FromResult(AuthenticateResult.Fail("User e/o password errati !!!"));
            }
            
            Credential credential = result;
            

            var authenticatedUser = new AuthenticatedUser("BasicAuthentication", true, credential.EmailAddress);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(authenticatedUser));

            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));

        }
    }
}
