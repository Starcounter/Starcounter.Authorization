using System;
using Simplified.Ring1;

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

    [Database]
    public class Role : Something
    {

    }
}