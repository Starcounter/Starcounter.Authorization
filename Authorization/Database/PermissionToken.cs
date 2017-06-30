using System.Linq;
using Simplified.Ring1;
using Simplified.Ring2;
using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.Database
{
    /// <summary>
    /// Used to associate <see cref="Permission"/> with <see cref="SomebodyGroup"/> in Database. Instance of this class
    /// represents a single Permission, by serializing it. This serialized form should never be used on its own, only compared
    /// to other serialized permissions - serialization implementation is subject to changes. To obtain an instance of this class
    /// it's recommended to use either <see cref="GetForPermissionOrNull{T}"/> when looking for existing permission or
    /// <see cref="GetForPermissionOrCreate{T}"/> when creating new one.
    /// </summary>
    [Database]
    public class PermissionToken : Something
    {
        private static readonly PermissionTokenSerializer Serializer = new PermissionTokenSerializer();

        /// <summary>
        /// Serialized form of permission that this instance is representing. Actual form may change in the future
        /// but is currently a JSON document with database fields represented as their base64 IDs
        /// </summary>
        public string SerializedPermission { get; set; }

        /// <summary>
        /// Full name of type of permission that this instance is representing.
        /// </summary>
        public string SerializedType { get; set; }

        /// <summary>
        /// Retrieve an instance representing given permission. Returns null if nothing is found.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public static PermissionToken GetForPermissionOrNull(Permission permission)
        {
            return Db.SQL<PermissionToken>(
                    $"select a from {typeof(PermissionToken).FullName} a where a.{nameof(SerializedPermission)} = ? and a.{nameof(SerializedType)} = ?",
                    Serialize(permission),
                    permission.GetType().ToString())
                .FirstOrDefault();
        }

        /// <summary>
        /// Retrieve an instance representing given permission. Creates one if it doesn't already exist.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public static PermissionToken GetForPermissionOrCreate(Permission permission)
        {
            return GetForPermissionOrNull(permission) ?? new PermissionToken
                   {
                       SerializedType = permission.GetType().ToString(),
                       SerializedPermission = Serialize(permission)
                   };
        }

        private static string Serialize<T>(T obj)
        {
            return Serializer.Serialize(obj);
        }
    }
}