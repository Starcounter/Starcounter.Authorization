using System;
using System.Collections.Generic;
using System.Linq;

namespace Starcounter.Authorization.Core.Rules
{
    public class AuthorizationRules : IAuthorizationConfigurator, IAuthorizationRulesSource
    {
        private readonly Dictionary<Type, List<object>> _predicates = new Dictionary<Type, List<object>>();

        public void AddRule<TPermission>(Func<IEnumerable<Claim>, TPermission, bool> predicate) where TPermission : Permission
        {
            GetPredicatesForPermission<TPermission>()
                .Add(predicate);
        }

        public IEnumerable<Func<IEnumerable<Claim>, TPermission, bool>> Get<TPermission>() where TPermission:Permission
        {
            return GetPredicatesForPermission<TPermission>()
                .Cast<Func<IEnumerable<Claim>, TPermission, bool>>();
        }

        private List<object> GetPredicatesForPermission<TPermission>()
        {
            List<object> p;
            if (!_predicates.TryGetValue(typeof(TPermission), out p))
            {
                p = new List<object>();
                _predicates[typeof(TPermission)] = p;
            }
            return p;
        }
    }
}