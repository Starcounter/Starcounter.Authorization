using System;
using System.Collections.Generic;
using System.Linq;

namespace Starcounter.Authorization.Core.Rules
{
    /// <summary>
    /// User possesing a TClaim will be granted TPermission if predicate evaluates to true.
    /// If user has more than one claims of type TClaim, he will be granted the permission if any of the claims passes the predicate.
    /// </summary>
    /// <typeparam name="TPermission"></typeparam>
    /// <typeparam name="TClaim"></typeparam>
    public class ClaimRule<TPermission, TClaim> : IAuthorizationRule<TPermission> where TPermission : Permission
    {
        private readonly Func<TClaim, TPermission, bool> _predicate;

        public ClaimRule(Func<TClaim, TPermission, bool> predicate)
        {
            _predicate = predicate;
        }

        public bool Evaluate(IEnumerable<Claim> claims, IAuthorizationEnforcement authorizationEnforcement, TPermission permission)
        {
            return claims.OfType<TClaim>().Any(claim => _predicate(claim, permission));
        }
    }
}