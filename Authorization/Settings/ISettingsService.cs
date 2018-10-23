
namespace Starcounter.Authorization.Settings
{
    /// <summary>
    /// Allows manipulation of settings regulating the behavior of Authorization Library.
    /// </summary>
    /// <typeparam name="TAuthorizationSettings">Database class defined in app, implementing <see cref="IAuthorizationSettings"/></typeparam>
    public interface ISettingsService<out TAuthorizationSettings>
        where TAuthorizationSettings : class, IAuthorizationSettings, new()
    {
        /// <summary>
        /// Returns the settings database object, throwing if the object is missing or more than one exists. This methods doesn't create its own transaction.
        /// </summary>
        /// <returns>Settings database object. This method never returns null. Returned object must not be deleted.</returns>
        TAuthorizationSettings GetSettings();

        /// <summary>
        /// Creates the settings database object if necessary and returns it. If more than one object is present in database,
        /// all will be deleted and one will be recreated.
        /// </summary>
        /// <returns></returns>
        TAuthorizationSettings EnsureSettings();
    }
}