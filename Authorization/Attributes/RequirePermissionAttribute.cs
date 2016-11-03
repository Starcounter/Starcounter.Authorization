using System;

namespace Starcounter.Authorization.Attributes
{
    /// <summary>
    /// Require specific permission to run a handler.
    /// Must be placed on a Handle(Input.) handler inside a Page (Json class) or the Page itself
    /// RequiredPermission must be of type Permission and have a default constructor
    /// Before calling the Handler / loading the Page the permission will be checked
    /// If placed on a Page, permission will be checked before any unmarked Handler is run 
    /// and before any Property is changed by the user
    /// </summary>
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
    public sealed class RequirePermissionAttribute : Attribute
    {
        public Type RequiredPermission { get; private set; }

        public RequirePermissionAttribute(Type requiredPermission)
        {
            RequiredPermission = requiredPermission;
        }
    }
}