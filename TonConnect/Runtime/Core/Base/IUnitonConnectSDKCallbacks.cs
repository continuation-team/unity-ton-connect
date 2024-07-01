using TonSdk.Connect;

namespace UnitonConnect.Core.Common
{
    public interface IUnitonConnectSDKCallbacks
    {
        delegate void OnUnitonConnectInitialize();

        delegate void OnWalletConnectionFinish(Wallet wallet);
        delegate void OnWalletConnectionFail(string errorMessage);

        delegate void OnWalletConnectionRestore(bool isRestored);
        delegate void OnWalletConnectionPause();
        delegate void OnWalletConnectionUnPause();

        delegate void OnSendTransactionFinish(bool isSuccess);

        delegate void OnWalletDisconnect();

        event OnUnitonConnectInitialize OnInitialized;

        event OnWalletConnectionFinish OnWalletConnectionFinished;
        event OnWalletConnectionFail OnWalletConnectionFailed;

        event OnWalletConnectionRestore OnWalletConnectionRestored;
        event OnWalletConnectionPause OnWalletConnectionPaused;
        event OnWalletConnectionUnPause OnWalletConnectonUnPaused;

        event OnSendTransactionFinish OnSendTransactionFinished;

        event OnWalletDisconnect OnWalletDisconnected;
    }
}