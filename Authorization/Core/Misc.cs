using Simplified.Ring1;

namespace Starcounter.Authorization.Core
{
    public interface ISecurePage
    {
        Json Init(params object[] args);
    }

    public abstract class Claim
    {
    }

    public abstract class Unit
    {
    }

    public abstract class Permission<T>
    {
    }

    [Database]
    public class Role : Something
    {

    }
}