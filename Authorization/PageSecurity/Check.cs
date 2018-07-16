using System;

namespace Starcounter.Authorization.PageSecurity
{
    /// <summary>
    /// A permission check that can be performed when given page instance.
    /// </summary>
    internal class Check
    {
        public Check(Type pageType, Delegate checkAction)
        {
            PageType = pageType;
            CheckAction = checkAction;
        }

        private Check()
        {
            AllowAnonymous = true;
        }

        public static Check CreateAllowAnonymous() => new Check();

        /// <summary>
        /// The type of the page that hosts the property guarded by this check.
        /// </summary>
        public Type PageType { get; }

        /// <summary>
        /// An executable piece of code that will perform the check. Accepts a single argument of type <see cref="PageType"/>, returns bool. 
        /// If the check is succesful, returns true. Otherwise will perform whatever <see cref="CheckersCreator._checkDeniedHandler"/> dictates and return false.
        /// </summary>
        public Delegate CheckAction { get; }

        /// <summary>
        /// If this is true, then everything that is guarded by this check should be not actually be checked.
        /// </summary>
        public bool AllowAnonymous { get; }
    }
}