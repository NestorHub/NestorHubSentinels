using NestorHub.Common.Api;

namespace NestorHub.Sentinels.Api.Class
{
    public class SubscriptionOnStateValue
    {
        public SubscriptionKey Key { get; }
        public StateValueKey StateValueKey { get; }

        public SubscriptionOnStateValue(SubscriptionKey key, StateValueKey stateValueKey)
        {
            Key = key;
            StateValueKey = stateValueKey;
        }
    }
}
