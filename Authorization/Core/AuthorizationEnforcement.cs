using System.Linq;

namespace Starcounter.Authorization.Core
{
    public class AuthorizationEnforcement
    {
        private readonly IAuthorizationRulesSource _rules;
        private readonly IAuthenticationBackend _authentication;

        public AuthorizationEnforcement(IAuthorizationRulesSource rules, IAuthenticationBackend authentication)
        {
            _rules = rules;
            _authentication = authentication;
        }

        public bool TryPermission<TPermission>(TPermission permission) where TPermission : Permission
        {
            var claims = _authentication.GetCurrentClaims().ToList();

            return _rules.Get<TPermission>()
                .Any(func => func(claims, permission));
        }

        public void TryPermissionOrThrow<TPermission>(TPermission permission) where TPermission : Permission
        {
            if (!TryPermission(permission))
            {
                throw new UnauthorizedException();
            }
        }
    }
}