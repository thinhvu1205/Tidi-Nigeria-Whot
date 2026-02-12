using System.Collections.Generic;
using System.Linq;
using Nakama;
using Proto;

public class FriendManager : Singleton<FriendManager>
{
    private HashSet<string> friendIdSet = new();

    public void SetFriendList(IApiFriendList list)
    {
        foreach(IApiFriend friend in list.Friends)
        {
            friendIdSet.Add(friend.User.Id);
        }
    }

    public bool IsFriend(string userId)
    {
        return friendIdSet.Contains(userId);
    }

    public void AddFriend(string userId)
    {
        friendIdSet.Add(userId);
    }

    public void RemoveFriend(string userId)
    {
        friendIdSet.Remove(userId);
    }
}