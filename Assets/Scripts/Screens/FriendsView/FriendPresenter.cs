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

    /// <summary>
    /// Load friend list theo tab và cursor (phân trang). Tab = FRIEND/CLOSE/BEST/SOULMATE/INVITE_SENT/INVITE_RECEIVED.
    /// </summary>
    public async UniTask<FriendListResponse> GetListFriend(FriendTab tab, string cursor = "")
    {
        FriendListResponse friendListResponse = await DataSender.GetListFriends(tab, cursor);
        if (friendListResponse != null)
            Debug.Log($"GetListFriend tab={tab} cursor={cursor?.Length ?? 0} count={friendListResponse.Friends?.Count ?? 0} nextCursor={!string.IsNullOrEmpty(friendListResponse.NextCursor)}");
        return friendListResponse;
    }

    /// <summary>
    /// Chỉ lấy tab counts (gọi khi vào UI Friends).
    /// </summary>
    public async UniTask<FriendListResponse> GetFriendTabCounts()
    {
        return await DataSender.GetFriendTabCounts();
    }

    public async UniTask<ListRecentConversationsResponse> GetListRecentConversations()
    {
        return await DataSender.GetListRecentConversations();
    }

    public async UniTask JoinDirectChat(string userId)
    {
        await NetworkManager.INSTANCE.JoinDirectChat(userId);

    }
    public async UniTask<IApiChannelMessageList> GetDirectChatHistory()
    {
        return await NetworkManager.INSTANCE.GetDirectChatHistory();
    }

    public async UniTask SendMessage(string message)
    {
        await NetworkManager.INSTANCE.SendMessageWorldChat(message);
    }

    
}

