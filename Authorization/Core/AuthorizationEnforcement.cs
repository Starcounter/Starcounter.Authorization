using System;
using System.Linq;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Core.Rules;

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

            var enablingRule = _rules.Get<TPermission>()
                .FirstOrDefault(rule => rule.Evaluate(claims, this, permission));
            Console.WriteLine($"Permission '{permission}' granted by rule '{enablingRule}'");
            return enablingRule != null;
        }
    }
}