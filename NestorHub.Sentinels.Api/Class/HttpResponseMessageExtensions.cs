using System.Net.Http;
using Newtonsoft.Json;

namespace NestorHub.Sentinels.Api.Class
{
    public static class HttpResponseMessageExtensions
    {
        public static T ContentAs<T>(this HttpResponseMessage response)
        {
            var data = response.Content.ReadAsStringAsync().Result;
            return string.IsNullOrEmpty(data) ?
                default(T) :
                JsonConvert.DeserializeObject<T>(data);
        }
    }
}
