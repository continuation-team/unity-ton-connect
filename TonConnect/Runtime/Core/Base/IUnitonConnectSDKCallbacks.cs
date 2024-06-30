using TonSdk.Connect;

namespace UnitonConnect.Core.Common
{
    public interface IUnitonConnectSDKCallbacks
    {
        delegate void OnWalletConnectionFinish(Wallet wallet);
        delegate void OnWalletConnectionFail(string errorMessage);

        delegate void OnWalletConnectionRestore(bool isRestored);

        event OnWalletConnectionFinish OnWalletConnectionFinished;
        event OnWalletConnectionFail OnWalletConnectionFailed;

        event OnWalletConnectionRestore OnWalletConnectionRestored;
    }
}