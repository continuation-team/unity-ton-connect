using TonSdk.Connect;

namespace UnitonConnect.Core.Common
{
    public interface IUnitonConnectSDKCallbacks
    {
        delegate void OnWalletConnectionFinish(Wallet wallet);
        delegate void OnWalletConnectionFail(string errorMessage);

        event OnWalletConnectionFinish OnWalletConnectionFinished;
        event OnWalletConnectionFail OnWalletConnectionFailed;
    }
}