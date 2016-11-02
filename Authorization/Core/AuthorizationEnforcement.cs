using System.Linq;
using Starcounter.Authorization.Authentication;

namespace Starcounter.Authorization.Core
{
    public class AuthorizationEnforcement : IAuthorizationEnforcement
    {
        private readonly IAuthorizationRulesSource _rules;
        private readonly IAuthenticationBackend _authentication;

        public AuthorizationEnforcement(IAuthorizationRulesSource rules, IAuthenticationBackend authentication)
        {
            _rules = rules;
            _authentication = authentication;
        }

        public bool CheckPermission<TPermission>(TPermission permission) where TPermission : Permission
        {
            var claims = _authentication.GetCurrentClaims().ToList();

            return _rules.Get<TPermission>()
                .Any(func => func(claims, permission));
        }
    }
}