namespace NestorHub.Sentinels.Api.Domain
{
    public class HomeHubConnection : HubConnectionBase
    {
        public HomeHubConnection(HomeConnectionServer connection)
            :base(connection, "homehub")
        {}
    }
}
