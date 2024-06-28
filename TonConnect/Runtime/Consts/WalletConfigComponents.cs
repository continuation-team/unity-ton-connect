using System.Collections.Generic;
using Newtonsoft.Json;
using TonSdk.Connect;

namespace UnitonConnect.Core.Data.Common
{
    public sealed class WalletConfigComponents
    {
        public const string NAME = "name";
        public const string APP_NAME = "app_name";
        public const string IMAGE = "image";
        public const string ABOUT_URL = "about_url";

        public const string BRIDGE = "bridge";
        public const string SSE = "sse";
        public const string JAVA_SCRIPT = "js";

        public static WalletConfig GetWalletConfig(Dictionary<string, object> wallet)
        {
            WalletConfig config = new()
            {
                Name = wallet[NAME].ToString(),
                Image = wallet[IMAGE].ToString(),
                AboutUrl = wallet[ABOUT_URL].ToString(),
                AppName = wallet[APP_NAME].ToString()
            };

            return config;
        }

        public static List<Dictionary<string, object>> GetBridgesFromWallet(
            Dictionary<string, object> wallet)
        {
            List<Dictionary<string, object>> bridges = JsonConvert.DeserializeObject<List<
                            Dictionary<string, object>>>(wallet[BRIDGE].ToString());

            return bridges;
        }
    }
}