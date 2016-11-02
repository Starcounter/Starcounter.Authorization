using System;

namespace Starcounter.Authorization.Routing.Middleware
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class UseDbScopeAttribute : Attribute
    {
        public bool Value { get; set; }

        public UseDbScopeAttribute(bool value = true)
        {
            Value = value;
        }
    }
}