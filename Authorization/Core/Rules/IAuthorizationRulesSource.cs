using System;
using System.Collections.Generic;

namespace Starcounter.Authorization.Core
{
    public interface IAuthorizationRulesSource
    {
        IEnumerable<Func<IEnumerable<Claim>, TPermission, bool>> Get<TPermission>() 
            where TPermission : Permission;
    }
}