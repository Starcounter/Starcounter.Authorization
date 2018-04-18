using System;

namespace Starcounter.Authorization
{
    public interface ISystemClock
    {
        DateTimeOffset UtcNow { get; }
    }
}