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
        /// <summary>
        /// Return the first and last characters of the wallet address
        /// </summary>
        /// <param name="address">Address of the connected wallet account</param>
        /// <param name="charAmount">Number of characters to display among the first and last</param>
        public static string ProcessWalletAddress(
            string address, int charAmount)
        {
            if (address.Length < 8)
            {
                return address;
            }

            string firstFourChars = address.Substring(0, charAmount);
            string lastFourChars = address.Substring(address.Length - charAmount);

            return firstFourChars + "..." + lastFourChars;
        }

        /// <summary>
        /// Download the wallet icon from the server using the specified link.
        /// </summary>
        /// <param name="imageUrl">Referencing a wallet icon from a previously retrieved wallet configuration via the `OnWalletConnectionFinished` event.</param>
        public static async Task<Texture2D> GetWalletIconFromServerAsync(
            string imageUrl)
        {
            Texture2D walletIcon = null;

            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
            {
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result != WebRequestUtils.SUCCESS)
                {
                    UnitonConnectLogger.LogError($"Failed to load wallet image with error: {request.error}");

                    return null;
                }

                walletIcon = DownloadHandlerTexture.GetContent(request);

                return walletIcon;
            }
        }

        /// <summary>
        /// Generate QR code from the link to connect the wallet
        /// </summary>
        /// <param name="connectUrl">Previously obtained connection link</param>
        public static Texture2D GetQRCodeFromUrl(string connectUrl)
        {
            return QRGenerator.EncodeString(connectUrl.ToString());
        }

        /// <summary>
        /// Get wallet icon from local storage, if it exists
        /// </summary>
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

        /// <summary>
        /// Gets the wallet data container to be used in the project interface, even if there is no icon in the local store
        /// </summary>
        /// <param name="config">Configuration of the previously obtained wallet</param>
        /// <param name="localStorage">Local storage of used wallets</param>
        public static async Task<WalletViewData> GetWalletViewIfIconIsNotExist(
            WalletConfig config, WalletsProvidersData localStorage)
        {
            string name = config.Name;
            Texture2D icon = null;

            if (UnitonConnectSDK.Instance.IsUseCachedWalletsIcons)
            {
                icon = await GetWalletIconFromLocalStorage(config,
                    localStorage.Config);
            }
            else
            {
                icon = await GetWalletIconFromServerAsync(config.Image);
            }

            return GetWalletView(name, icon);
        }

        /// <summary>
        /// Get a sprite from a 2D texture
        /// </summary>
        /// <param name="texture">Texture to be converted to a sprite</param>
        public static Sprite GetSpriteFromTexture(Texture2D texture)
        {
            return Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }

        /// <summary>
        /// Get the wallet data container to be used in the project interface.
        /// </summary>
        /// <param name="name">Name of the previously received wallet</param>
        /// <param name="icon">Icon of the previously received wallet</param>
        public static WalletViewData GetWalletView(string name,
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