using System;

namespace Starcounter.Authorization.Routing
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class UrlAttribute : Attribute
    {
        public string Value { get; private set; }

        public UrlAttribute(string value)
        {
            Value = value;
        }
    }
}