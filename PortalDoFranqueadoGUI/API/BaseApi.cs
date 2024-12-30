namespace PortalDoFranqueado.Api
{
    internal static class BaseApi
    {
        public static SimpleHttpClientRequest<T> GetSimpleHttpClientRequest<T>(string uriComplement)
        {
            var fullUrl = string.Concat(Configuration.Current.UrlBase, uriComplement);

            return new SimpleHttpClientRequest<T>()
            {
                RequestUri = fullUrl,
                BearerToken = Configuration.Current.Session?.Token
            };
        }

        public static SimpleHttpClientRequest GetSimpleHttpClientRequest(string uriComplement)
        {
            var fullUrl = string.Concat(Configuration.Current.UrlBase, uriComplement);

            return new SimpleHttpClientRequest()
            {
                RequestUri = fullUrl,
                BearerToken = Configuration.Current.Session?.Token
            };
        }
    }
}
