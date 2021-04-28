using System;
using Microsoft.ApplicationInsights.NLogTarget;
using NestorHub.Sentinels.Api.Domain;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace NestorHub.Sentinels.Api
{
    public class LoggerHost
    {
        private readonly SentinelHost _sentinelHost;
        private readonly HomeConnectionServer _homeConnectionServer;
        private static readonly object padlock = new object();

        private LoggerHost(SentinelHost sentinelHost, HomeConnectionServer homeConnectionServer)
        {
            _sentinelHost = sentinelHost;
            _homeConnectionServer = homeConnectionServer;

            LogManager.Configuration = CreateLoggingConfiguration();
        }

        public static LoggerHost GetLoggerHost(SentinelHost sentinelHost, HomeConnectionServer homeConnectionServer)
        {
            lock (padlock)
            {
                return new LoggerHost(sentinelHost, homeConnectionServer);
            }
        }

        public void LogInformation(string message)
        {
            var logger = LogManager.GetLogger(_sentinelHost.GetName());
            logger.Info(AddAdditionalInformation(message));
        }

        public void LogWarning(string message)
        {
            var logger = LogManager.GetLogger(_sentinelHost.GetName());
            logger.Warn(AddAdditionalInformation(message));
        }

        public void LogError(Exception exception, string message)
        {
            var logger = LogManager.GetLogger(_sentinelHost.GetName());
            logger.Error(exception, AddAdditionalInformation(message));
        }

        private string AddAdditionalInformation(string message)
        {
            return
                $"{message} - Sentinel : {_sentinelHost.GetName()} - Server : {_homeConnectionServer.GetUrlConnectionOnServer()}";
        }

        private static LoggingConfiguration CreateLoggingConfiguration()
        {
            var config = new LoggingConfiguration();
            var target = new ApplicationInsightsTarget
            {
                InstrumentationKey = "555b5423-78ac-4329-929c-f5f93439f3c7"
            };
            var rule = new LoggingRule("*", LogLevel.Debug, target);
            var targetConsole = new ConsoleTarget();
            var ruleConsole = new LoggingRule("*", LogLevel.Debug, targetConsole);
            config.LoggingRules.Add(rule);
            config.LoggingRules.Add(ruleConsole);
            return config;
        }
    }
}
