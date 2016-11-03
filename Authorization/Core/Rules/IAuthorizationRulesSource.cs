using System;
using System.Collections.Generic;

namespace Starcounter.Authorization.Core.Rules
{
    public interface IAuthorizationRulesSource
    {
        IEnumerable<IAuthorizationRule<TPermission>> Get<TPermission>()
            where TPermission : Permission;
    }
}