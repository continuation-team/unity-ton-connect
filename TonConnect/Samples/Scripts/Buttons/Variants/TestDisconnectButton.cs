using UnitonConnect.Core.Demo;
using UnitonConnect.Core.Utils.Debugging;

namespace UnitonConnect.Core.Data
{
    public sealed class TestDisconnectButton : TestBaseButton
    {
        public sealed override async void OnClick()
        {
            UnitonConnectLogger.Log("The disconnecting process of the previously connected wallet has been started");

            await UnitonConnectSDK.Instance.DisconnectWallet();

            UnitonConnectLogger.Log("Success disconnect");

        }
    }
}