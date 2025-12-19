using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Globals;
using Nakama;
using Proto;
using UnityEngine;

public class PlayerProfileInGamePresenter
{
    private PlayerProfileInGameView playerProfileInGameView;

    public void Init(PlayerProfileInGameView view)
    {
        playerProfileInGameView = view;
    }

    public async UniTask SendEmoji(string senderId, string receiverId, string emojiId)
    {
        await NetworkManager.INSTANCE.SendEmojiToPlayerRoomChat(senderId, receiverId, emojiId);
    }

    
}

