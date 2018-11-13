using System;

namespace Starcounter.Authorization.PageSecurity
{
    /// <summary>
    /// Provides a single instance of <see cref="CheckersCache"/>. Used because different applications can create checkers for the same, shared view-models.
    /// </summary>
    internal class CheckersCacheProvider
    {
        private static readonly Lazy<CheckersCache> Lazy = new Lazy<CheckersCache>();
        public static CheckersCache Instance => Lazy.Value;
    }
}