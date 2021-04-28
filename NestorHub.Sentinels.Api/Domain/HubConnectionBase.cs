namespace NestorHub.Sentinels.Api.Domain
{
    public class HubConnectionBase
    {
        private readonly HomeConnectionServer _connection;
        private readonly string _hubName;

        protected HubConnectionBase(HomeConnectionServer connection, string hubName)
        {
            _connection = connection;
            _hubName = hubName;
        }

        public string GetUrl()
        {
            return !string.IsNullOrEmpty(_hubName) ? $"{_connection.GetUrlConnectionOnServer()}/{_hubName}" : $"{_connection.GetUrlConnectionOnServer()}";
        }

        public string GetUrlForSignalRHubs()
        {
            return $"{_connection.GetUrlConnectionOnServer()}";
        }
    }
}