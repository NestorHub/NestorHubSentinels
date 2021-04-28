using System;

namespace NestorHub.Sentinels.Api.Domain
{
    public class HomeConnectionServer
    {
        private readonly string _urlServer;
        private readonly int _port;
        private readonly bool _useSsl;

        public static HomeConnectionServer CreateConnection(string urlServer, int port, bool useSsl = false)
        {
            if (!string.IsNullOrEmpty(urlServer) && port < 0)
            {
                throw new ArgumentException("Url or port doesn't have empty or zero on value");
            }
            return new HomeConnectionServer(urlServer, port, useSsl);
        }

        private HomeConnectionServer(string urlServer, int port, bool useSsl = false)
        {
            _urlServer = urlServer;
            _port = port;
            _useSsl = useSsl;
        }

        internal string GetUrlConnectionOnServer()
        {
            var protocol = _useSsl ? "https" : "http";
            return $"{protocol}://{_urlServer}:{_port}";
        }
    }
}