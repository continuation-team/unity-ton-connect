using UnityEngine.Networking;
using UnitonConnect.Core.Data;
using UnitonConnect.Runtime.Data;
using UnitonConnect.Editor.Common;
using UnitonConnect.Core.Utils.Debugging;

namespace UnitonConnect.Core.Utils
{
    public sealed class WebRequestUtils
    {
        public const UnityWebRequest.Result SUCCESS = UnityWebRequest.Result.Success;

        public const UnityWebRequest.Result IN_PROGRESS = UnityWebRequest.Result.InProgress;

        public const UnityWebRequest.Result CONNECTION_ERROR = UnityWebRequest.Result.ConnectionError;
        public const UnityWebRequest.Result PROTOCOL_ERROR = UnityWebRequest.Result.ProtocolError;
        public const UnityWebRequest.Result DATA_PROCESSING_ERROR = UnityWebRequest.Result.DataProcessingError;

        public const string HEADER_CONTENT_TYPE = "Content-Type";
        public const string HEADER_ACCEPT = "Accept";

        public const string HEADER_VALUE_TEXT_EVENT_STREAM = "text/event-stream";
        public const string HEADER_VALUE_TEXT_PLAIN = "text/plain";

        public static void SetRequestHeader(UnityWebRequest webRequest,
            string header, string headerValue)
        {
            webRequest.SetRequestHeader(header, headerValue);
        }

        public static string GetGatewaySenderLink(GatewayMessageData gatewayMessage)
        {
            return $"{gatewayMessage.BridgeUrl}/{gatewayMessage.PostPath}?" +
                $"client_id={gatewayMessage.SessionId}&to={gatewayMessage.Receiver}" +
                $"&ttl={gatewayMessage.TimeToLive}&topic={gatewayMessage.Topic}";
        }

        public static string GetAppManifestLink(bool isTesting, DAppConfig config)
        {
            var dAppManifestLink = ProjectStorageConsts.GetTestAppManifest();

            if (!isTesting && config == null)
            {
                UnitonConnectLogger.LogError("Failed to detect the configuration of your dApp" +
                    " to generate the manifest. It can be assigned via the `Uniton Connect -> dApp Config` configuration window");

                return string.Empty;
            }

            if (!isTesting)
            {
                dAppManifestLink = ProjectStorageConsts.GetAppManifest(config.Data.ProjectLink,
                    ProjectStorageConsts.APP_DATA_FILE_NAME);
            }

            return dAppManifestLink;
        }
    }
}
