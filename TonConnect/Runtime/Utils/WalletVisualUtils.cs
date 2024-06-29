using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TonSdk.Connect;
using UnitonConnect.Core.Data;
using UnitonConnect.Core.Utils.Debugging;

namespace UnitonConnect.Core.Utils.View
{
    public sealed class WalletVisualUtils
    {
        public static string ProcessWalletAddress(string address)
        {
            if (address.Length < 8)
            {
                return address;
            }

            string firstFourChars = address.Substring(0, 6);
            string lastFourChars = address.Substring(address.Length - 6);

            return firstFourChars + "..." + lastFourChars;
        }

        /// <summary>
        /// Download the wallet icon from the server using the specified link.
        /// </summary>
        /// <param name="imageUrl">Referencing a wallet icon from a previously retrieved wallet configuration via the `OnWalletConnectionFinished` event.</param>
        public static async Task<Texture2D> GetWalletIconFromServerAsync(string imageUrl)
        {
            Texture2D walletIcon = null;

            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
            {
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == WebRequestUtils.CONNECTION_ERROR ||
                    request.result == WebRequestUtils.PROTOCOL_ERROR)
                {
                    UnitonConnectLogger.LogError($"Failed to load wallet image with error: {request.error}");

                    return null;
                }

                walletIcon = DownloadHandlerTexture.GetContent(request);

                return walletIcon;
            }
        }

        public static Texture2D GetQRCodeFromUrl(string connectUrl)
        {
            return QRGenerator.EncodeString(connectUrl.ToString());
        }

        /// <summary>
        /// Get wallet icon from local storage, if it exists
        /// </summary>
        /// <param name="targetWalletName">Wallet name identifier for receiving the icon</param>
        public static async Task<Texture2D> GetWalletIconFromLocalStorage(
            WalletConfig config, List<WalletProviderConfig> localStorage)
        {
            if (!UnitonConnectSDK.Instance.IsUseCachedWalletsIcons)
            {
                UnitonConnectLogger.LogWarning("For loading wallet icons from local storage, " +
                    "you need to activate the 'Use Cached Wallets Icons' option");

                return null;
            }

            Texture2D icon = null;

            foreach (var wallet in localStorage)
            {
                if (wallet.Data.Name == config.AppName)
                {
                    icon = wallet.Data.Icon;
                }
            }

            if (icon == null)
            {
                UnitonConnectLogger.LogError($"Failed to load {config.Name} wallet icon to local storage, start downloading from server...");

                icon = await GetWalletIconFromServerAsync(config.Image);

                UnitonConnectLogger.Log($"{config.Name} wallet icon successfully downloaded from the server");
            }

            return icon;
        }

        public static Sprite GetSpriteFromTexture(Texture2D texture)
        {
            Debug.Log("created sprite");

            return Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }

        public static WalletViewData GetNewWalletView(string name,
            Texture2D icon)
        {
            var supportedWalletView = new WalletViewData()
            {
                Name = name,
                Icon = icon
            };

            return supportedWalletView;
        }
    }
}