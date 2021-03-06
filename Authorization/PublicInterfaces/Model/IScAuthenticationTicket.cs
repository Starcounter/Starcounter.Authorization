﻿using System;

namespace Starcounter.Authorization
{
    /// <summary>
    /// Implement this interface with a database class specific to your application.
    /// </summary>
    public interface IScAuthenticationTicket
    {
        /// <summary>
        /// in UTC. After this time passes this ticket can and should be removed
        /// </summary>
        DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Used to persist the ticket between window reloads. Generated with secure RNG. Stored in cookies.
        /// </summary>
        string PersistenceToken { get; set; }
    }
}