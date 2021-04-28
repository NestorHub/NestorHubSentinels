using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NestorHub.Sentinels.Api.Class;

namespace NestorHub.Sentinels.Api.Domain
{
    public class HomeConnectionClient : HttpClient
    {
        private readonly HomeHubConnection _homeHubConnection;

        public HomeConnectionClient(HomeHubConnection homeConnectionServer)
        {
            _homeHubConnection = homeConnectionServer;
        }

        public async Task<T> PostAsJson<T>(string action, object value)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.PostAsync($"{_homeHubConnection.GetUrl()}/{action}", new JsonContent(value));
            return response.ContentAs<T>();
        }

        public async Task<T> Delete<T>(string action, string value)
        {
            var client = new HttpClient();
            var response = await client.DeleteAsync($"{_homeHubConnection.GetUrl()}/{action}/{value}");
            return response.ContentAs<T>();
            
        }
    }
}
