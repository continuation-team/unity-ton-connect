using System;
using System.Threading;
using TonSdk.Connect;

namespace UnitonConnect.Core.Data
{
    [Serializable]
    public sealed class EventListenerArgumentsData
    {
        public CancellationToken Token { get; set; }
        public string Url { get; set; }
        public ProviderMessageHandler Provider { get; set; }
        public ProviderErrorHandler ErrorProvider { get; set; }
    }
}