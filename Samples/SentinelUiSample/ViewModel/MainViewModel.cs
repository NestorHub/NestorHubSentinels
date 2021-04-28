using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NestorHub.Common.Api;
using NestorHub.Common.Api.Enum;
using NestorHub.Sentinels.Api;
using NestorHub.Sentinels.Api.Domain;

namespace SentinelUiSample.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private SubscriptionHost _subscriptionHost;

        private bool _sentinelRun;
        public bool SentinelRun
        {
            get => _sentinelRun;
            set
            {
                _sentinelRun = value;
                RaisePropertyChanged(() => SentinelRun);
            }
        }

        private bool _isSubscribed;
        public bool IsSubscribed
        {
            get => _isSubscribed;
            set
            {
                _isSubscribed = value;
                RaisePropertyChanged(() => IsSubscribed);
            }
        }

        private string _connectionMessage;
        private HomeConnectionServer _homeConnectionServer;
        private StatesValuesHost _statesValuesHost;

        public string ConnectionMessage
        {
            get => _connectionMessage;
            set
            {
                _connectionMessage = value;
                RaisePropertyChanged(() => ConnectionMessage);
            }
        }

        public string ClientName { get; set; }
        public string PackageName { get; set; }
        public string HomeControllerAddress { get; set; }
        public int HomeControllerPort { get; set; }
        public string StateValueSentinelName { get; set; }
        public string StateValuePackageName { get; set; }
        public string StateValueName { get; set; }
        public List<TypeOfValue> TypesOfValue { get; set; }
        public string Value { get; set; }
        public TypeOfValue SelectedTypeOfValue { get; set; }
        private StateValueKey _addedStateValueKey;

        public StateValueKey AddedStateValueKey
        {
            get => _addedStateValueKey;
            set
            {
                _addedStateValueKey = value;
                RaisePropertyChanged(() => AddedStateValueKey);
            }
        }
        public ObservableCollection<StateValue> StateValues { get; set; }
        public RelayCommand Run { get; set; }
        public RelayCommand Subscribe { get; set; }
        public RelayCommand Unsubscribe { get; set; }
        public RelayCommand Send { get; set; }
        public RelayCommand Delete { get; set; }

        public MainViewModel()
        {
            Run = new RelayCommand(RunSentinel);
            Subscribe = new RelayCommand(SubscribeOnStateValue);
            Unsubscribe = new RelayCommand(UnsubscribeOnStateValue);
            Send = new RelayCommand(SendStateValue);
            Delete = new RelayCommand(DeleteStateValue);
            StateValues = new ObservableCollection<StateValue>();
            HomeControllerAddress = "localhost";
            HomeControllerPort = 8006;

            TypesOfValue = new List<TypeOfValue>()
            {
                TypeOfValue.Int,
                TypeOfValue.Double,
                TypeOfValue.String,
                TypeOfValue.Undefined
            };
        }

        private async void SendStateValue()
        {
            var valueOfState = ConvertValueToType(SelectedTypeOfValue, Value);
            AddedStateValueKey = await _statesValuesHost.Send(StateValueName, valueOfState);
        }

        private async void DeleteStateValue()
        {
            if (await _statesValuesHost.Delete(AddedStateValueKey))
            {
                AddedStateValueKey = null;
            }
        }

        private object ConvertValueToType(TypeOfValue typeOfValue, object value)
        {
            switch (typeOfValue)
            {
                case TypeOfValue.Int:
                    return Convert.ToInt32(value);
                case TypeOfValue.Double:
                    return Convert.ToDouble(value);
                case TypeOfValue.String:
                    return Convert.ToString(value);
                case TypeOfValue.Undefined:
                    default:
                    return (dynamic) value;
            }
        }

        private void SubscribeOnStateValue()
        {
            SubscribeOnStateValue(StateValueSentinelName, StateValuePackageName, StateValueName);
        }

        private void UnsubscribeOnStateValue()
        {
            UnsubscribeOnStateValue(StateValueSentinelName, StateValuePackageName, StateValueName);
        }

        private void RunSentinel()
        {
            var sentinelHost = new SentinelHost();

            sentinelHost.Run(ClientName, PackageName);

            _homeConnectionServer = HomeConnectionServer.CreateConnection(HomeControllerAddress, HomeControllerPort);

            _subscriptionHost = new SubscriptionHost(sentinelHost, _homeConnectionServer);
            _subscriptionHost.SubscriptionValueChanged += SubscriptionHostOnSubscriptionValueChanged;
            _subscriptionHost.HomeControllerConnectionOpened += HomeControllerConnectionOpened;
            _subscriptionHost.HomeControllerConnectionClosed += HomeControllerConnectionClosed;

            _statesValuesHost = new StatesValuesHost(sentinelHost, _homeConnectionServer);

            SentinelRun = true;
        }

        private void HomeControllerConnectionClosed()
        {
            ConnectionMessage = "Lost connection with Home Controller Server";
        }

        private void HomeControllerConnectionOpened()
        {
            ConnectionMessage = "Opened connection with Home Controller Server";
        }

        private async void SubscribeOnStateValue(string sentinelName, string packageName, string valueName)
        {
            if (await _subscriptionHost.IsOnline())
            {
                var subscribeId = await _subscriptionHost.Subscribe(sentinelName, packageName, valueName);
            }
        }

        private async void UnsubscribeOnStateValue(string sentinelName, string packageName, string valueName)
        {
            IsSubscribed = !(await _subscriptionHost.UnSubscribe(sentinelName, packageName, valueName));
        }

        private void SubscriptionHostOnSubscriptionValueChanged(StateValueKey stateValueKey, StateValue value)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StateValues.Add(value);
            });
        }
    }
}
