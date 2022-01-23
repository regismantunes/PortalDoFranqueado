namespace PortalDoFranqueadoAPI.Repositories.Util
{
    public static class ObjectExtensions
    {
        public static object? ToDBValue(this object? obj)
            => obj is null ? DBNull.Value : obj;
    }
}
