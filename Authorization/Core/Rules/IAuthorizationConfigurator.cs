using System;
using System.Collections.Generic;
using Starcounter.Authorization.Core.Rules;

namespace Starcounter.Authorization.Core
{
    /// <summary>
    /// Implementors allow for configuration of authorization rules.
    /// <seealso cref="AuthorizationConfiguratorExtensions"/>
    /// </summary>
    public interface IAuthorizationConfigurator
    {
        /// <summary>
        /// Add an authorization rule for given permission. Multiple rules for the same permission are logically
        /// summed (any of them passing grant access)
        /// </summary>
        /// <seealso cref="AuthorizationConfiguratorExtensions"/>
        /// <typeparam name="TPermission">Type of the permission that is configured</typeparam>
        /// <param name="predicate">The rule deciding if acccess should be granted. Accepts a collection of available claims and permission in question.
        /// Return true to denote that access should be granted, false otherwise.</param>
        void AddRule<TPermission>(Func<IEnumerable<Claim>, TPermission, bool> predicate) 
            where TPermission : Permission;
    }
}