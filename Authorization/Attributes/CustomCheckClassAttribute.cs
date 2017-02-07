using System;
using Starcounter.Authorization.Routing;

namespace Starcounter.Authorization.Attributes
{
    /// <summary>
    /// Denotes a method used to obtain a Permission to check.
    /// </summary>
    /// Use this attribute to mark a public, static method accepting page context (see <see cref="IPageContext{T}"/> and 
    /// returning a Permission.
    /// This method will be used to obtain a Permission to check against when determining if a user can access a page
    /// and when user interacts with the page. 
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CustomCheckClassAttribute : Attribute
    {
    }
}