using System;

namespace Starcounter.Authorization.Attributes
{
    /// <summary>
    /// Require specific permission to run a handler or access a Page.
    /// Must be placed on a Handle(Input.) handler inside an IBound&lt;T&gt; or the IBound Page itself
    /// RequiredPermission must be of type Permission&lt;T&gt;
    /// Before calling the Handler / loading the Page the permission will be checked against Data of the Page
    /// If placed on a Page, permission will be checked before any unmarked Handler is run 
    /// and before any Property is changed by the user
    /// </summary>
    public sealed class RequirePermissionDataAttribute : Attribute
    {
        public Type RequiredPermission { get; private set; }

        public RequirePermissionDataAttribute(Type requiredPermission)
        {
            RequiredPermission = requiredPermission;
        }
    }
}