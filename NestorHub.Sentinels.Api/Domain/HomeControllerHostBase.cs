using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NestorHub.Sentinels.Api.Class;

namespace NestorHub.Sentinels.Api.Domain
{
    public class HomeControllerHostBase
    {
        protected readonly SentinelHost SentinelHost;
        protected readonly HomeHubConnection HomeHubConnection;
        protected LoggerHost Logger { get; }

        protected HomeControllerHostBase(SentinelHost sentinelHost, HomeConnectionServer homeConnectionServer)
        {
            SentinelHost = sentinelHost;
            HomeHubConnection = new HomeHubConnection(homeConnectionServer);
            Logger = LoggerHost.GetLoggerHost(sentinelHost, homeConnectionServer);
        }

        public async Task<bool> IsOnline()
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.GetAsync($"{HomeHubConnection.GetUrl()}/isonline");
                return response.ContentAs<bool>();
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}