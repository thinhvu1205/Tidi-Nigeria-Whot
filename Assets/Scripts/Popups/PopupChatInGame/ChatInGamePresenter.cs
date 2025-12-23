using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Globals;
using Nakama;
using Proto;
using UnityEngine;

public class ChatInGamePresenter
{
    private ChatInGameView chatInGameView;

    public void Init(ChatInGameView view)
    {
        chatInGameView = view;
    }

    public async UniTask SendMessage(string message)
    {
        await NetworkManager.INSTANCE.SendMessageRoomChat(message);
    }

    public async UniTask SendChatVoice(string senderId, string text, int index = 0, int totalPart = 0, long time = -1)
    {
        await NetworkManager.INSTANCE.SendChatVoice(senderId, text, index, totalPart, time);
    }

    
}

