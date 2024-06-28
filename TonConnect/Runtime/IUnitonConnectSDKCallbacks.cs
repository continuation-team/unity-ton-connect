using TonSdk.Connect;

namespace UnitonConnect.Core.Common
{
    public interface IUnitonConnectSDKCallbacks
    {
        delegate void OnProviderStatusChange(Wallet wallet);
        delegate void OnProviderStatusFail(string errorMessage);

        event OnProviderStatusChange OnProviderStatusChanged;
        event OnProviderStatusFail OnProviderStatusFailed;
    }
}