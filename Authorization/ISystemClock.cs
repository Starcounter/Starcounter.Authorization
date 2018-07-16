using System;

namespace Starcounter.Authorization
{
    internal interface ISystemClock
    {
        DateTimeOffset UtcNow { get; }
    }
}