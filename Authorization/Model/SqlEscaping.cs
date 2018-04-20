using System.Text;

namespace Starcounter.Authorization.Model
{
    public static class SqlEscaping
    {
        private static string EscapeSingleIdentifier(string word)
        {
            return $"\"{word}\"";
        }

        /// <summary>
        /// Escapes an identifier to be used in Starcounter SQL.
        /// </summary>
        public static string EscapeSql(this string word)
        {
            var parts = word.Split('.');
            if (parts.Length == 1)
            {
                return EscapeSingleIdentifier(word);
            }

            var sb = new StringBuilder();
            foreach (var identifier in parts)
            {
                sb.Append('"');
                sb.Append(identifier);
                sb.Append('"');
                sb.Append('.');
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }
    }
}