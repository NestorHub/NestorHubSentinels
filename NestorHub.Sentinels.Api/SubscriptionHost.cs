using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NestorHub.Common.Api;
using NestorHub.Sentinels.Api.Class;
using NestorHub.Sentinels.Api.Domain;

namespace NestorHub.Sentinels.Api
{
    public delegate void SubscriptionValueChangedEventHandler(StateValueKey stateValueKey, StateValue value);
    public delegate void SubscriptionHubConnectionOpenedEventHandler();
    public delegate void SubscriptionHubConnectionClosedEventHandler();

    public class SubscriptionHost : HomeControllerHostBase
    {
        public event SubscriptionValueChangedEventHandler SubscriptionValueChanged;
        public event SubscriptionHubConnectionOpenedEventHandler HomeControllerConnectionOpened;
        public event SubscriptionHubConnectionClosedEventHandler HomeControllerConnectionClosed;

        private readonly Dictionary<StateValueKey, SubscriptionOnStateValue> _subscriptionsStore = new Dictionary<StateValueKey, SubscriptionOnStateValue>();
        
        private SubscriptionHubConnection _subscriptionHubConnection;

        public SubscriptionHost(SentinelHost sentinelHost, HomeConnectionServer homeConnectionServer) : base(
            sentinelHost, homeConnectionServer)
        {
            InitiateSubscriptionHubConnection();
        }

        public async Task<SubscriptionOnStateValue> Subscribe(string sentinelHost, string packageHost, string stateValueName)
        {
            using (var client = new HomeConnectionClient(HomeHubConnection))
            {
                var stateValueKey = new StateValueKey(sentinelHost, packageHost, stateValueName);
                await SubscribeOnValueChanged(stateValueKey);
                var subscriptionKeyValue = await client.PostAsJson<string>("subscription",
                    new Subscription(SentinelHost.GetName(), sentinelHost, packageHost, stateValueName));
                if (!string.IsNullOrEmpty(subscriptionKeyValue))
                {
                    var subscriptionKey = new SubscriptionKey(subscriptionKeyValue);
                    if (subscriptionKey.IsValid())
                    {
                        var subscriptionOnStateValue = new SubscriptionOnStateValue(subscriptionKey, stateValueKey);
                        AddSubscriptionInStore(stateValueKey, subscriptionOnStateValue);
                        
                        return subscriptionOnStateValue;
                    }
                }
                else
                {
                    await UnSubscribeOnValueChanged(new StateValueKey(sentinelHost, packageHost, stateValueName));
                }
            }

            return null;
        }

        public async Task<bool> UnSubscribe(string sentinelHost, string packageHost, string stateValueName)
        {
            using (var client = new HomeConnectionClient(HomeHubConnection))
            {
                var stateValueKey = new StateValueKey(sentinelHost, packageHost, stateValueName);
                if (_subscriptionsStore.ContainsKey(stateValueKey))
                {
                    var isSubscriptionDeleted = await client.Delete<bool>("subscription",
                        $"{SentinelHost.GetName()}.{_subscriptionsStore[stateValueKey].Key}");
                    if (isSubscriptionDeleted)
                    {
                        RemoveSubscriptionToStore(stateValueKey);
                        await UnSubscribeOnValueChanged(new StateValueKey(sentinelHost, packageHost, stateValueName));
                    }
                    return isSubscriptionDeleted;
                }
            }
            return false;
        }

        private void InitiateSubscriptionHubConnection()
        {
            _subscriptionHubConnection = new SubscriptionHubConnection(HomeHubConnection, Logger);
            _subscriptionHubConnection.HubConnectionOpened += SubscriptionHubConnectionOpened;
            ;
            _subscriptionHubConnection.HubConnectionClosed += SubscriptionHubConnectionClosed;
            _subscriptionHubConnection.ValueChanged += SetValueChange;
        }

        private async void SubscriptionHubConnectionOpened()
        {
            if (_subscriptionsStore.Any())
            {
                await ResubscribeAfterLostServerConnection();
            }
            HomeControllerConnectionOpened?.Invoke();
            Logger.LogInformation($"Connection opened");
        }

        private void SubscriptionHubConnectionClosed()
        {
            HomeControllerConnectionClosed?.Invoke();
            Logger.LogInformation($"Connection closed");
        }

        private async Task ResubscribeAfterLostServerConnection()
        {
            var stateValueKeyToResubscribe = new List<StateValueKey>(_subscriptionsStore.Select(s => s.Key));
            _subscriptionsStore.Clear();
            foreach (var stateValueKey in stateValueKeyToResubscribe)
            {
                (string sentinelName, string packageName, string name) = stateValueKey.GetComponents();
                await Subscribe(sentinelName, packageName, name);
            }
        }

        private void AddSubscriptionInStore(StateValueKey stateValueKey, SubscriptionOnStateValue subscriptionOnStateValue)
        {
            if (_subscriptionsStore.ContainsKey(stateValueKey))
            {
                _subscriptionsStore[stateValueKey] = subscriptionOnStateValue;
            }
            else
            {
                _subscriptionsStore.Add(stateValueKey, subscriptionOnStateValue);
            }
        }

        private void RemoveSubscriptionToStore(StateValueKey stateValueKey)
        {
            if (_subscriptionsStore.ContainsKey(stateValueKey))
            {
                _subscriptionsStore.Remove(stateValueKey);
            }
        }

        private async Task SubscribeOnValueChanged(StateValueKey stateValueKey)
        {
            await _subscriptionHubConnection.AddToGroup(stateValueKey);
            Logger.LogInformation($"Subscribe to state value {stateValueKey}");
        }

        private async Task UnSubscribeOnValueChanged(StateValueKey stateValueKey)
        {
            await _subscriptionHubConnection.RemoveToGroup(stateValueKey);
            Logger.LogInformation($"Unsubscribe to state value {stateValueKey}");

        }

        private void SetValueChange(StateValueKey stateValueKey, StateValue value)
        {
            SubscriptionValueChanged?.Invoke(stateValueKey, value);
            Logger.LogInformation($"Subscription value receive for {stateValueKey} with value {value.Value}, updating on {value.LastUpdate}");
        }
    }
}
