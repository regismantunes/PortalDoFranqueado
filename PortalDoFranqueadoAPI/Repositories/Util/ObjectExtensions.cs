using System;
using System.Data;
using System.Data.Common;

namespace PortalDoFranqueadoAPI.Repositories.Util
{
    public static class ObjectExtensions
    {
        public static dynamic ToDBValue<T>(this T obj)
            => obj is null ? DBNull.Value : obj;

        public static dynamic ToDBValue(this string[]? obj)
        {
            if (obj == null ||
                obj.Length == 0)
                return DBNull.Value;

            var text = string.Empty;
            foreach (var item in obj)
                text += string.Concat(item, '|');

            return text[0..^1];
        }

        public static int ToZeroIfNull(this int? obj)
            => obj is null ? 0 : obj.Value;

        public static string[]? GetStringArray(this DbDataReader reader, string name)
        {
            var value = reader.GetValue(name);
            if (value == null ||
                value is not string text)
                return null;

            if (string.IsNullOrEmpty(text))
                return null;

            return text.Split('|');
        }
    }
}
