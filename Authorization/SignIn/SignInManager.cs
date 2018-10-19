using System;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.SignIn
{
    internal class SignInManager<TAuthenticationTicket, TUser> : ISignInManager<TUser>
        where TAuthenticationTicket : IScUserAuthenticationTicket<TUser>
        where TUser : IMinimalUser
    {
        private readonly IAuthenticationTicketService<TAuthenticationTicket> _authenticationTicketService;
        private readonly ILogger _logger;

        public SignInManager(IAuthenticationTicketService<TAuthenticationTicket> authenticationTicketService,
            ILogger<SignInManager<TAuthenticationTicket, TUser>> logger)
        {
            _authenticationTicketService = authenticationTicketService;
            _logger = logger;
        }

        public void SignIn(TUser user)
        {
            try
            {
                var authenticationTicket = _authenticationTicketService.EnsureTicket();
                authenticationTicket.User = user;
                _logger.LogInformation("User {User} signed in", user);
            }
            catch (Exception e)
            {
                _logger.LogError("Could not sign in user {User}", e, user);
                throw;
            }
        }
    }
}