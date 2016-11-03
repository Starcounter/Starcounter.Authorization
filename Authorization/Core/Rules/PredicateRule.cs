using System;
using System.Collections.Generic;

namespace Starcounter.Authorization.Core.Rules
{
    public class PredicateRule<TPermission> : IAuthorizationRule<TPermission> where TPermission : Permission
    {
        private readonly Func<IEnumerable<Claim>, IAuthorizationEnforcement, TPermission, bool> _predicate;

        public PredicateRule(Func<IEnumerable<Claim>, IAuthorizationEnforcement, TPermission, bool> predicate)
        {
            _predicate = predicate;
        }

        public bool Evaluate(IEnumerable<Claim> claims, IAuthorizationEnforcement authorizationEnforcement, TPermission permission)
        {
            return _predicate(claims, authorizationEnforcement, permission);
        }
    }
}