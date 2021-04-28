using System;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.SignalR.Client;
using NestorHub.Common.Api;

namespace NestorHub.Sentinels.Api.Domain
{
    public delegate void ValueChangedEventHandler(StateValueKey stateValueKey, StateValue value);
    public delegate void HubConnectionOpenedEventHandler();
    public delegate void HubConnectionClosedEventHandler();

    public class SubscriptionHubConnection
    {
        private readonly LoggerHost _logger;
        public event ValueChangedEventHandler ValueChanged;
        public event HubConnectionOpenedEventHandler HubConnectionOpened;
        public event HubConnectionClosedEventHandler HubConnectionClosed;

        private const int WaitDelayForServerRestarting = 5000;
        private const int NumberOfRetryConnection = 6;

        private readonly HubConnection _connection;
        private int _numberOfTries = 1;
        private Timer _timerToReconnect;

        public SubscriptionHubConnection(HomeHubConnection homeHubConnection, LoggerHost logger)
        {
            _logger = logger;
            _connection = new HubConnectionBuilder()
                .WithUrl($"{homeHubConnection.GetUrlForSignalRHubs()}/subscriptionshub")
                .Build();
            OpenHubConnection();
            _connection.Closed += ConnectionClosed;
        }

        public async Task AddToGroup(StateValueKey stateValueKey)
        {
            if (_connection.State == HubConnectionState.Disconnected)
            {
                _logger.LogWarning($"Restart connection due to connection closed, on AddToGroup function");
                await _connection.StartAsync();
            }
            await _connection.InvokeAsync("AddToGroup", stateValueKey.Key);
        }

        public async Task RemoveToGroup(StateValueKey stateValueKey)
        {
            if (_connection.State == HubConnectionState.Disconnected)
            {
                _logger.LogWarning($"Restart connection due to connection closed, on RemoveToGroup function");
                await _connection.StartAsync();
            }
            await _connection.InvokeAsync("RemoveToGroup", stateValueKey.Key);
        }

        private async void OpenHubConnection()
        {
            if (_connection.State == HubConnectionState.Disconnected)
            {
                _connection.On<StateValueKey, StateValue>("ValueChanged", StateValueChanged);

                try
                {
                    await _connection.StartAsync();
                    if (_connection.State == HubConnectionState.Connected)
                    {
                        HubConnectionOpened?.Invoke();
                    }
                }
                catch (Exception)
                {
                    HubConnectionClosed?.Invoke();
                }
            }
        }

        private void StateValueChanged(StateValueKey stateValueKey, StateValue value)
        {
            ValueChanged?.Invoke(stateValueKey, value);
        }

        private Task ConnectionClosed(Exception arg)
        {
            HubConnectionClosed?.Invoke();
            InitiateTimerToTryToReconnectOnServer();
            return Task.CompletedTask;
        }

        private void InitiateTimerToTryToReconnectOnServer()
        {
            _numberOfTries = 0;
            _timerToReconnect = new Timer(WaitDelayForServerRestarting);
            _timerToReconnect.Elapsed += TryToReconnectOnServer;
            _timerToReconnect.Start();
        }

        private async void TryToReconnectOnServer(object sender, ElapsedEventArgs e)
        {
            if (_numberOfTries <= NumberOfRetryConnection)
            {
                try
                {
                    _logger.LogWarning($"Try to reconnect to server - {_numberOfTries} attempt");

                    await _connection.StartAsync();
                    if (_connection.State == HubConnectionState.Connected)
                    {
                        HubConnectionOpened?.Invoke();
                        StopTriesToReconnect();
                    }
                }
                catch (Exception)
                {
                    _numberOfTries++;
                }
            }
            else
            {
                StopTriesToReconnect();
            }
        }

        private void StopTriesToReconnect()
        {
            _timerToReconnect.Stop();
            _timerToReconnect = null;
        }
    }
}
