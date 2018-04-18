namespace Starcounter.Authorization.Routing.Activation
{
    /// <summary>
    /// Pages implementing this interface have custom initialization that should be called directly
    /// after the construction. This interface is used by <see cref="DefaultPageCreator"/>,
    /// but can also be used by any custom page creators
    /// </summary>
    public interface IInitPage
    {
        void Init();
    }
}