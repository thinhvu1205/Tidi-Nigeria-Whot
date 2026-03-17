using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Globals;
using Nakama;
using Proto;
using UnityEngine;

public class EmojiInGamePresenter
{
    private EmojiInGameView emojiInGameView;

    public void Init(EmojiInGameView view)
    {
        emojiInGameView = view;
    }

    public async UniTask SendEmoji(string emojiId)
    {
        await NetworkManager.INSTANCE.SendEmoji(emojiId, User.UserAccount.Profile.UserId);
    }

    
}

