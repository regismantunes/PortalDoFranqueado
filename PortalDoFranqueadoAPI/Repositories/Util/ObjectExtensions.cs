using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace PortalDoFranqueadoAPI.Repositories.Util
{
    public static class ObjectExtensions
    {
        public static dynamic ToDBValue<T>(this T obj)
            => obj is null ? DBNull.Value : obj;

        public static dynamic ToDBValue(this IEnumerable<string> obj)
        {
            if (obj == null ||
                !obj.Any())
                return DBNull.Value;

            return string.Join('|', obj);
        }

        public static int ToZeroIfNull(this int? obj)
            => obj ?? 0;

        public static IEnumerable<string>? GetStringArray(this DbDataReader reader, string name)
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
