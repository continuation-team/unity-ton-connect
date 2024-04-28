using System;
using System.Collections;
using System.Collections.Generic;
using TonSdk.Connect;
using TonSdk.Core;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using Message = TonSdk.Connect.Message;

public class UIManager : MonoBehaviour
{
    [Tooltip("Toggle if you want to use presaved wallet icons. (recommended)")]
    public bool UseSavedWalletIcons = true;
    [Tooltip("Wallet icons. Works only if UseSavedWalletIcons is enabled.")]
    public List<Sprite> WalletIcons = new ();
    private List<string> WalletsIconsList = new () {"tonkeeper", "tonhub", "openmask", "dewallet", "mytonwallet", "tonflow", "tonwallet", "xtonwallet", "telegram-wallet"};

    [Header("UI References")]
    [SerializeField] private UIDocument document;
    [SerializeField] private VisualTreeAsset walletItem;

    [Header("References")]
    [SerializeField] private TonConnectHandler tonConnectHandler;
    
    private void Awake()
    {
        TonConnectHandler.OnProviderStatusChanged += OnProviderStatusChange;
        TonConnectHandler.OnProviderStatusChangedError += OnProviderStatusChangeError;
        DisableSendTXModal();
        DisableWalletInfoButton();
        EnableConnectWalletButton();
    }

    private void OnProviderStatusChange(Wallet wallet)
    {
        if(tonConnectHandler.tonConnect.IsConnected)
        {
            Debug.Log("Wallet connected. Address: " + wallet.Account.Address + ". Platform: " + wallet.Device.Platform + "," + wallet.Device.AppName + "," + wallet.Device.AppVersion);
            CloseConnectModal();
            DisableConnectWalletButton();
            EnableWalletInfoButton(ProcessWalletAddress(wallet.Account.Address.ToString(AddressType.Base64)));
        }
        else
        {
            EnableConnectWalletButton();
            DisableWalletInfoButton();
        }
    }

    private void OnProviderStatusChangeError(string message)
    {

    }

    #region Utilities

    private string ProcessWalletAddress(string address)
    {
        if (address.Length < 8) return address;

        string firstFourChars = address[..4];
        string lastFourChars = address[^4..];

        return firstFourChars + "..." + lastFourChars;
    }

    #endregion

    #region Button Click Events
    private async void OpenWalletQRContent(ClickEvent clickEvent, WalletConfig config)
    {
        document.rootVisualElement.Q<Label>("ConnectModal_Title").text = "Connect Wallet";

        document.rootVisualElement.Q<VisualElement>("ModalContent").style.display = DisplayStyle.None;
        document.rootVisualElement.Q<VisualElement>("ModalContentWeb").style.display = DisplayStyle.None;
        document.rootVisualElement.Q<VisualElement>("ModalQRContent").style.display = DisplayStyle.Flex;
        document.rootVisualElement.Q<VisualElement>("Button_Back").style.display = DisplayStyle.Flex;

        document.rootVisualElement.Q<Label>("ModalQRContent_OpenWalletButton_Title").text = $"Open {config.Name}";

        string connectUrl = await tonConnectHandler.tonConnect.Connect(config);
        Texture2D qrCodeTexture = QRGenerator.EncodeString(connectUrl.ToString());

        document.rootVisualElement.Q<VisualElement>("ModalQRContent_QRHandler").style.backgroundImage = new StyleBackground(qrCodeTexture);
        document.rootVisualElement.Q<VisualElement>("ModalQRContent_OpenWalletButton").UnregisterCallback<ClickEvent, string>(OpenWalletUrl);
        document.rootVisualElement.Q<VisualElement>("ModalQRContent_OpenWalletButton").RegisterCallback<ClickEvent, string>(OpenWalletUrl, connectUrl);
    }

    private async void OpenWebWallet(ClickEvent clickEvent, WalletConfig config)
    {
        await tonConnectHandler.tonConnect.Connect(config);
    }

    private void OpenWalletUrl(ClickEvent clickEvent, string url)
    {
        string escapedUrl = Uri.EscapeUriString(url);
        Application.OpenURL(escapedUrl);
    }

    private void CloseConnectModal(ClickEvent clickEvent) => CloseConnectModal();

    private void BackToMainContent(ClickEvent clickEvent)
    {
        tonConnectHandler.tonConnect.PauseConnection();
        document.rootVisualElement.Q<Label>("ConnectModal_Title").text = "Choose Wallet";
        document.rootVisualElement.Q<VisualElement>("ModalContent").style.display = DisplayStyle.Flex;
        document.rootVisualElement.Q<VisualElement>("ModalContentWeb").style.display = DisplayStyle.Flex;
        document.rootVisualElement.Q<VisualElement>("ModalQRContent").style.display = DisplayStyle.None;
        document.rootVisualElement.Q<VisualElement>("Button_Back").style.display = DisplayStyle.None;
    }

    private void ConnectWalletButtonClick(ClickEvent clickEvent)
    {
        ShowConnectModal();
    }

    private async void DisconnectWalletButtonClick(ClickEvent clickEvent)
    {
        EnableConnectWalletButton();
        DisableWalletInfoButton();
        tonConnectHandler.RestoreConnectionOnAwake = false;
        await tonConnectHandler.tonConnect.Disconnect();
    }

    private  void WalletInfoButtonClick(ClickEvent clickEvent)
    {
        ShowSendTXModal();
    }

    private void CloseTXModalButtonClick(ClickEvent clickEvent)
    {
        DisableSendTXModal();
    }

    private async void SendTXModalSendButtonClick(ClickEvent clickEvent)
    {
        string receiverAddress = document.rootVisualElement.Q<TextField>("SendTXModal_Address").value;
        double sendValue = document.rootVisualElement.Q<DoubleField>("SendTXModal_Value").value;
        if(string.IsNullOrEmpty(receiverAddress) || sendValue <= 0) return;

        Address receiver = new(receiverAddress);
        Coins amount = new(sendValue);
        Message[] sendTons = 
        {
            new Message(receiver, amount),
            //new Message(receiver, amount),
            //new Message(receiver, amount),
            //new Message(receiver, amount),
        };

        long validUntil = DateTimeOffset.Now.ToUnixTimeSeconds() + 600;

        SendTransactionRequest transactionRequest = new SendTransactionRequest(sendTons, validUntil);
        await tonConnectHandler.tonConnect.SendTransaction(transactionRequest);
    }

    #endregion

    #region Tasks
    private void LoadWalletsCallback(List<WalletConfig> wallets)
    {
        // Here you can do something with the wallets list
        // for example: add them to the connect modal window
        // Warning! Use coroutines to load data from the web
        StartCoroutine(LoadWalletsIntoModal(wallets));
    }

    private IEnumerator LoadWalletsIntoModal(List<WalletConfig> wallets)
    {
        VisualElement contentElement = document.rootVisualElement.Q<VisualElement>("ModalContent");
        VisualElement jsContentElement = document.rootVisualElement.Q<VisualElement>("ModalContentWeb");
        document.rootVisualElement.Q<Label>("ConnectModal_Title").text = "Choose Wallet";
        contentElement.Clear();
        jsContentElement.Clear();
        contentElement.style.display = DisplayStyle.None;
        jsContentElement.style.display = DisplayStyle.None;

        // load http bridge wallets
        foreach (var t in wallets)
        {
            if(t.BridgeUrl == null) continue;
            VisualElement walletElement = walletItem.CloneTree();

            if(UseSavedWalletIcons && WalletsIconsList.Contains(t.AppName))
            {
                walletElement.Q<VisualElement>("WalletButton_WalletImage").style.backgroundImage = new StyleBackground(WalletIcons[WalletsIconsList.IndexOf(t.AppName)]);
            }
            else
            {
                using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(t.Image))
                {
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) 
                        Debug.LogError("Error while loading wallet image: " + request.error);
                    else
                    {
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        if (texture != null) walletElement.Q<VisualElement>("WalletButton_WalletImage").style.backgroundImage = new StyleBackground(texture);
                    }
                }
            }

            walletElement.Q<Label>("WalletButton_WalletName").text = t.Name;
            walletElement.RegisterCallback<ClickEvent, WalletConfig>(OpenWalletQRContent, t);
            contentElement.Add(walletElement);
        }

        // load js bridge wallets
        if(tonConnectHandler.UseWebWallets)
        {
            for (int i = 0; i < wallets.Count; i++)
            {
                if(wallets[i].JsBridgeKey == null || !InjectedProvider.IsWalletInjected(wallets[i].JsBridgeKey)) continue;
                VisualElement walletElement = walletItem.CloneTree();

                if(UseSavedWalletIcons && WalletsIconsList.Contains(wallets[i].AppName))
                {
                    walletElement.Q<VisualElement>("WalletButton_WalletImage").style.backgroundImage = new StyleBackground(WalletIcons[WalletsIconsList.IndexOf(wallets[i].AppName)]);
                }
                else
                {
                    using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(wallets[i].Image))
                    {
                        yield return request.SendWebRequest();

                        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) 
                            Debug.LogError("Error while loading wallet image: " + request.error);
                        else
                        {
                            Texture2D texture = DownloadHandlerTexture.GetContent(request);
                            if (texture != null) walletElement.Q<VisualElement>("WalletButton_WalletImage").style.backgroundImage = new StyleBackground(texture);
                        }
                    }
                }

                walletElement.Q<Label>("WalletButton_WalletName").text = wallets[i].Name;
                walletElement.RegisterCallback<ClickEvent, WalletConfig>(OpenWebWallet, wallets[i]);
                jsContentElement.Add(walletElement);
            }
        }
        
        contentElement.style.display = DisplayStyle.Flex;
        jsContentElement.style.display = DisplayStyle.Flex;
    }
    #endregion

    #region UI Methods

    public void ShowConnectModal()
    {
        if (tonConnectHandler.tonConnect.IsConnected)
        {
            Debug.LogWarning("Wallet already connected. The connection window has not been opened. Before proceeding, please disconnect from your wallet.");
            document.rootVisualElement.Q<VisualElement>("ConnectModal").style.display = DisplayStyle.None;
            return;
        }
        tonConnectHandler.CreateTonConnectInstance();

        document.rootVisualElement.Q<Label>("ConnectModal_Title").text = "Choose Wallet";

        document.rootVisualElement.Q<VisualElement>("ModalContent").style.display = DisplayStyle.Flex;
        document.rootVisualElement.Q<VisualElement>("ModalContentWeb").style.display = DisplayStyle.Flex;
        document.rootVisualElement.Q<VisualElement>("ModalQRContent").style.display = DisplayStyle.None;
        document.rootVisualElement.Q<VisualElement>("Button_Back").style.display = DisplayStyle.None;
        document.rootVisualElement.Q<VisualElement>("ConnectModal").style.display = DisplayStyle.Flex;

        document.rootVisualElement.Q<VisualElement>("Button_Back").UnregisterCallback<ClickEvent>(BackToMainContent);
        document.rootVisualElement.Q<VisualElement>("Button_Back").RegisterCallback<ClickEvent>(BackToMainContent);
        document.rootVisualElement.Q<VisualElement>("Button_Close").UnregisterCallback<ClickEvent>(CloseConnectModal);
        document.rootVisualElement.Q<VisualElement>("Button_Close").RegisterCallback<ClickEvent>(CloseConnectModal);
        
        StartCoroutine(tonConnectHandler.LoadWallets("https://raw.githubusercontent.com/ton-blockchain/wallets-list/main/wallets-v2.json", LoadWalletsCallback));
    }


    private void CloseConnectModal()
    {
        if(!tonConnectHandler.tonConnect.IsConnected) tonConnectHandler.tonConnect.PauseConnection();
        document.rootVisualElement.Q<VisualElement>("ConnectModal").style.display = DisplayStyle.None;
    }

    private void EnableConnectWalletButton()
    {
        // enable connect button
        document.rootVisualElement.Q<VisualElement>("ConnectWalletButton").UnregisterCallback<ClickEvent>(ConnectWalletButtonClick);
        document.rootVisualElement.Q<VisualElement>("ConnectWalletButton").RegisterCallback<ClickEvent>(ConnectWalletButtonClick);
        document.rootVisualElement.Q<VisualElement>("ConnectWalletButton").style.display = DisplayStyle.Flex;
    }

    private void EnableWalletInfoButton(string wallet)
    {
        // enable wallet info and disconnect button
        document.rootVisualElement.Q<VisualElement>("WalletInfoButton").UnregisterCallback<ClickEvent>(WalletInfoButtonClick);
        document.rootVisualElement.Q<VisualElement>("WalletInfoButton").RegisterCallback<ClickEvent>(WalletInfoButtonClick);
        document.rootVisualElement.Q<VisualElement>("WalletInfoButton").style.display = DisplayStyle.Flex;

        document.rootVisualElement.Q<VisualElement>("DisconnectWalletButton").UnregisterCallback<ClickEvent>(DisconnectWalletButtonClick);
        document.rootVisualElement.Q<VisualElement>("DisconnectWalletButton").RegisterCallback<ClickEvent>(DisconnectWalletButtonClick);
        document.rootVisualElement.Q<VisualElement>("DisconnectWalletButton").style.display = DisplayStyle.Flex;

        document.rootVisualElement.Q<Label>("WalletInfoButton_Title").text = wallet;
    }

    private void DisableConnectWalletButton()
    {
        // disable connect button
        document.rootVisualElement.Q<VisualElement>("ConnectWalletButton").style.display = DisplayStyle.None;
    }

    private void DisableWalletInfoButton()
    {
        // disable wallet info and disconnect button
        document.rootVisualElement.Q<VisualElement>("WalletInfoButton").style.display = DisplayStyle.None;
        document.rootVisualElement.Q<VisualElement>("DisconnectWalletButton").style.display = DisplayStyle.None;
    }

    private void ShowSendTXModal()
    {
        document.rootVisualElement.Q<VisualElement>("SendTXModal").style.display = DisplayStyle.Flex;
        document.rootVisualElement.Q<VisualElement>("SendTXModal_Button_Close").UnregisterCallback<ClickEvent>(CloseTXModalButtonClick);
        document.rootVisualElement.Q<VisualElement>("SendTXModal_Button_Close").RegisterCallback<ClickEvent>(CloseTXModalButtonClick);
        document.rootVisualElement.Q<VisualElement>("SendTXModal_ConfirmButton").UnregisterCallback<ClickEvent>(SendTXModalSendButtonClick);
        document.rootVisualElement.Q<VisualElement>("SendTXModal_ConfirmButton").RegisterCallback<ClickEvent>(SendTXModalSendButtonClick);
    }

    private void DisableSendTXModal()
    {
        document.rootVisualElement.Q<VisualElement>("SendTXModal").style.display = DisplayStyle.None;
    }
    #endregion
}

   

  