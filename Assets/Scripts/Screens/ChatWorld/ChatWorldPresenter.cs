using System;
using Cysharp.Threading.Tasks;
using Globals;
using Proto;
using UnityEngine;

public class ChatWorldPresenter
{
    private ChatWorldView chatWorldView;

    public void Init(ChatWorldView view)
    {
        chatWorldView = view;
    }

    public async UniTask SendMessage(string message)
    {
        await NetworkManager.INSTANCE.SendMessageWorldChat(message);
    }

    
}

