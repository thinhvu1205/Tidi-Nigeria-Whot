using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Globals;
using Nakama;
using Proto;
using UnityEngine;

public class FriendPresenter
{
    private FriendsView friendView;

    public void Init(FriendsView view)
    {
        friendView = view;
    }

    public async UniTask<FriendListResponse> GetListFriend()
    {
        FriendListResponse friendListResponse = await DataSender.GetListFriends();
        Debug.Log("friendListResponse:" + friendListResponse);
        return friendListResponse;
    }

    public async UniTask SendMessage(string message)
    {
        await NetworkManager.INSTANCE.SendMessageWorldChat(message);
    }

    
}

