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

    public async UniTask<List<IApiChannelMessage>> GetWorldChatHistory(string nextCursor = "")
    {
        IApiChannelMessageList result = await NetworkManager.INSTANCE.GetWorldChatHistory(nextCursor);
        chatWorldView.nextCursor = result.NextCursor;
        chatWorldView.prevCursor = result.PrevCursor;
        UIManager.Instance.HideProgressing();
        return result.Messages.Reverse().ToList();
    }

    public async UniTask SendMessage(string message)
    {
        await NetworkManager.INSTANCE.SendMessageWorldChat(message);
    }

    
}

