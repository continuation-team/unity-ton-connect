mergeInto(LibraryManager.library, 
{
    IsWalletInjected: function (injectedWalletKey) 
    {
        var keyString = UTF8ToString(injectedWalletKey);
        return (keyString in window && typeof window[keyString] === 'object' && 'tonconnect' in window[keyString])      
    },
    IsInsideWalletBrowser: function (injectedWalletKey)
    {
        var keyString = UTF8ToString(injectedWalletKey);
        if(keyString in window && typeof window[keyString] === 'object' && 'tonconnect' in window[keyString])
        {
            return window[keyString].tonconnect.isWalletBrowser;
        }
        return false;
    },
    CallConnect: async function(request, version, injectedWalletKey)
    {
        var requestString = UTF8ToString(request);
        var requestObj = JSON.parse(requestString);
        var keyString = UTF8ToString(injectedWalletKey);
        var result;
        try
        {
            var data = JSON.stringify(await window[keyString].tonconnect.connect(version, requestObj));
            result = 
            {
                type: "event",
                data: data
            };
            console.log(result);
        }
        catch(e)
        {
            var connectEventError = {
                event: 'connect_error',
                payload: {
                    code: 0,
                    message: e.toString()
                }
            };

            var data = JSON.stringify(connectEventError);
            result = 
            {
                type: "event",
                data: data
            };
            console.log(e);
        }
        unityInstanceRef.SendMessage("TonConnect_Core", "OnInjectedWalletMessageReceived", JSON.stringify(result));
    },
    CallListenEvents: async function(injectedWalletKey)
    {
        var keyString = UTF8ToString(injectedWalletKey);
        unsubscribe = await window[keyString].tonconnect.listen(e =>
        {
            var result = 
            {
                type: "event",
                data: JSON.stringify(e)
            };
            console.log('Wallet message received:', result);
            unityInstanceRef.SendMessage("TonConnect_Core", "OnInjectedWalletMessageReceived", JSON.stringify(result));
        })
    },
    CallRestoreConnection: async function(injectedWalletKey, requestId)
    {
        var keyString = UTF8ToString(injectedWalletKey);
        var requestKey = UTF8ToString(requestId);
        var result;
        try
        {
            var data = JSON.stringify(await window[keyString].tonconnect.restoreConnection());
            result = { type: "restore", data: data, id: requestKey };
            console.log(result);
        }
        catch(e)
        {
            console.log(e);
            result = { type: "restore", data: "", id: requestKey };
        }
        unityInstanceRef.SendMessage("TonConnect_Core", "OnInjectedWalletMessageReceived", JSON.stringify(result));
    },
    CallSendRequest: async function(requestString, injectedWalletKey, requestId)
    {
        var keyString = UTF8ToString(injectedWalletKey);
        var requestKey = UTF8ToString(requestId);

        try
        {
            var request = JSON.parse(UTF8ToString(requestString));
            var tx = 
            {
                method: request.method,
                id: request.id,
                params: JSON.parse(request.params)
            }
            console.log(tx);
            const resultSend = window[keyString].tonconnect.send(request);
            resultSend.then(response => 
            {
                var data = JSON.stringify(response);
                var result = { type: "send", data: data, id: requestKey };
                unityInstanceRef.SendMessage("TonConnect_Core", "OnInjectedWalletMessageReceived", JSON.stringify(result));
            }); 
        }
        catch(e)
        {
            console.log('Send request error: ', e)
            var result = { type: "send", data: "", id: requestKey };
            unityInstanceRef.SendMessage("TonConnect_Core", "OnInjectedWalletMessageReceived", JSON.stringify(result));
        }
    },
    CallDisconnect: async function(injectedWalletKey)
    {
        var keyString = UTF8ToString(injectedWalletKey);
        try
        {
            window[keyString].tonconnect.disconnect();
        }
        catch(e)
        {
            console.log(e);
            
            const result = window[keyString].tonconnect.send(
            {
                method: 'disconnect',
                params: []
            });
            result.then(response => console.log('Wallet message received:', response));
        }
    }, 
});