using System;
using System.Composition;
using System.Threading.Tasks;
using System.Timers;
using NestorHub.Common.Api.Mef;
using NestorHub.Sentinels.Api;
using NestorHub.Sentinels.Api.Class;
using NestorHub.Sentinels.Api.Domain;

namespace PublishStateValueSample
{
    [Sentinel("PublishStateValueSample")]
    public class PublishStateValue : SentinelHost, ISentinel
    {
        private StatesValuesHost _statesValuesHost;
        private Timer _timerToReconnect;
        private RangeOfValues _rangeOfValue;

        [ImportingConstructor]
        public PublishStateValue()
        {}

        public async Task<bool> Run(string sentinelName, string packageName, string homeControllerUrl, int homeControllerPort, bool useSsl, object parameter)
        {
            base.Run(sentinelName, packageName);

            _rangeOfValue = GetRangeOfValues(parameter);
            _statesValuesHost = new StatesValuesHost(this, HomeConnectionServer.CreateConnection(homeControllerUrl, homeControllerPort, useSsl));

            SetTimeToSendValue(parameter);
            return true;
        }

        public async Task<bool> Stop()
        {
           _timerToReconnect.Stop();
           _timerToReconnect = null;
           return true;
        }

        private void SetTimeToSendValue(object parameter)
        {
            var intervalToSend = parameter.GetPropertyValue<int>("interval");
            _timerToReconnect = new Timer(intervalToSend);
            _timerToReconnect.Elapsed += SendValue;
            _timerToReconnect.Start();
        }

        private async void SendValue(object sender, ElapsedEventArgs e)
        {
            var random = new Random();
            await _statesValuesHost.Send("random", random.Next(_rangeOfValue.Start, _rangeOfValue.End));
        }

        private RangeOfValues GetRangeOfValues(object parameter)
        {
            if (parameter != null)
            {
                return new RangeOfValues(parameter.GetPropertyValue<int>("start"), parameter.GetPropertyValue<int>("end"));
            }
            return new RangeOfValues(10, 25);
        }
    }

    internal class RangeOfValues
    {
        public int Start { get; private set; }
        public int End { get; private set; }

        public RangeOfValues(int start, int end)
        {
            Start = start;
            End = end;
        }
    }
}
