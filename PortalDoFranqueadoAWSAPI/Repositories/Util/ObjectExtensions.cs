using System;

namespace PortalDoFranqueadoAPIAWS.Repositories.Util
{
    public static class ObjectExtensions
    {
        public static dynamic ToDBValue<T>(this T obj)
            => obj is null ? DBNull.Value : obj;

        public static int ToZeroIfNull(this int? obj)
            => obj is null ? 0 : obj.Value;
    }
}
