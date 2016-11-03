using System;
using System.Collections.Generic;

namespace Starcounter.Authorization.Core.Rules
{
    public class PermissionGroupRule<TPermission, TPermissionGroup> : IAuthorizationRule<TPermission>
        where TPermission : Permission
        where TPermissionGroup : Permission
    {
        private readonly Func<TPermission, TPermissionGroup> _groupSelection;

        public PermissionGroupRule(Func<TPermission, TPermissionGroup> groupSelection)
        {
            _groupSelection = groupSelection;
        }

        public bool Evaluate(IEnumerable<Claim> claims, IAuthorizationEnforcement authorizationEnforcement,
            TPermission permission)
        {
            return authorizationEnforcement.CheckPermission(_groupSelection(permission));
        }
    }
}