using System;

namespace UnitonConnect.Core.Data
{
    [Serializable]
    public sealed class GatewayMessageData
    {
        public string BridgeUrl { get; set; }
        public string PostPath { get; set; }
        public string SessionId { get; set; }
        public string Receiver { get; set; }
        public int TimeToLive { get; set; }
        public string Topic { get; set; }
        public byte[] Message { get; set; }
    }
}