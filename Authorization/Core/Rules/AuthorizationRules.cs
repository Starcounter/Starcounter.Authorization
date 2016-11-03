using System;
using System.Collections.Generic;
using System.Linq;

namespace Starcounter.Authorization.Core.Rules
{
    public class AuthorizationRules : IAuthorizationConfigurator, IAuthorizationRulesSource
    {
        private readonly Dictionary<Type, List<object>> _predicates = new Dictionary<Type, List<object>>();

        public void AddRule<TPermission>(IAuthorizationRule<TPermission> rule) where TPermission : Permission
        {
            GetPredicatesForPermission<TPermission>()
                .Add(rule);
        }

        public IEnumerable<IAuthorizationRule<TPermission>> Get<TPermission>() where TPermission:Permission
        {
            return GetPredicatesForPermission<TPermission>()
                .Cast<IAuthorizationRule<TPermission>>();
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