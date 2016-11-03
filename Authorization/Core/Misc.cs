using System;

namespace Starcounter.Authorization.Core
{
    public abstract class Claim
    {
    }

    public abstract class Permission
    {
    }

    public class UnauthorizedException : Exception
    {
    }
}