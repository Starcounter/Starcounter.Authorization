namespace Starcounter.Authorization.Routing
{
    /// <summary>
    /// Use to specify the Context type for page class.
    /// </summary>
    /// Context is a representation of data passed in URI parameters.
    /// It's usually the object that a page is bound to (so its type is usually T in <see cref="IBound{T}"/>).
    /// You need to use this interface if the Context type could not be inferred automatically (it's different then your page is IBound to)
    /// or you want to specify custom Context handling (maybe your context is actually a pair of db objects and they need to land in specific properties of your page)
    /// <typeparam name="T">The type of the Context</typeparam>
    // This is also a marker interface, to make it covariant is undesired
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IPageContext<T>
    {
        void HandleContext(T context);
    }
}