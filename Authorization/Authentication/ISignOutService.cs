﻿namespace Starcounter.Authorization.Authentication
{
    internal interface ISignOutService
    {
        /// <summary>
        /// Signs the user out by removing the current authentication ticket. Does nothing if the user wasn't signed in.
        /// </summary>
        void SignOut();
    }
}