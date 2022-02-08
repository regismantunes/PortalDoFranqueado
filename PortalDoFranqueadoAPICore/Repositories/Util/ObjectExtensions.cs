using System;

namespace PortalDoFranqueadoAPICore.Repositories.Util
{
    public static class ObjectExtensions
    {
        public static dynamic ToDBValue<T>(this T obj)
            => obj is null ? (object)DBNull.Value : obj;

        public static int ToZeroIfNull(this int? obj)
            => obj is null ? 0 : obj.Value;
    }
}
