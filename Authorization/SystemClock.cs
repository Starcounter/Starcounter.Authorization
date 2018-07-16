using System;

namespace Starcounter.Authorization
{
    internal class SystemClock : ISystemClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}