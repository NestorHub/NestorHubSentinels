using System.Threading.Tasks;
using NestorHub.Common.Api;
using NestorHub.Common.Api.Class;
using NestorHub.Sentinels.Api.Domain;

namespace NestorHub.Sentinels.Api
{
    public class StatesValuesHost : HomeControllerHostBase
    {
        private readonly SentinelHost _sentinelHost;

        public StatesValuesHost(SentinelHost sentinelHost, HomeConnectionServer homeConnectionServer) 
            : base(sentinelHost, homeConnectionServer)
        {
            _sentinelHost = sentinelHost;
        }

        public async Task<StateValueKey> Send(string name, object value)
        {
            return await Send(_sentinelHost.GetName(), _sentinelHost.GetPackageName(), name, value);
        }

        public async Task<StateValueKey> Send(string sentinelName, string packageName, string name, object value)
        {
            var typeOfValue = value.GetType().GetTypeOfValue();

            var stateValue = new StateValue(sentinelName, packageName, name, value, typeOfValue);

            using (var client = new HomeConnectionClient(HomeHubConnection))
            {
               return await client.PostAsJson<StateValueKey>("statesvalues", stateValue);
            }
        }

        public async Task<bool> Delete(StateValueKey stateValueKey)
        {
            using (var client = new HomeConnectionClient(HomeHubConnection))
            {
               return await client.Delete<bool>("statesvalues", stateValueKey.Key);
            }
        }
    }
}
