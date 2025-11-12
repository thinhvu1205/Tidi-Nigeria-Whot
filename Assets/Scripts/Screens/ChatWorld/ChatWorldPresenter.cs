using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Globals;
using Nakama;
using Proto;
using UnityEngine;

public class ChatWorldPresenter
{
    private ChatWorldView chatWorldView;

    public void Init(ChatWorldView view)
    {
        chatWorldView = view;
    }

    public async UniTask<List<IApiChannelMessage>> GetWorldChatHistory()
    {
        IApiChannelMessageList result = await NetworkManager.INSTANCE.GetWorldChatHistory();
        return result.Messages.Reverse().ToList();;
    }

    public async UniTask SendMessage(string message)
    {
        await NetworkManager.INSTANCE.SendMessageWorldChat(message);
    }

    
}

