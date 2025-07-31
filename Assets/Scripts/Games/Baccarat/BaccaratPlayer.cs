using System;
using UnityEngine;

[System.Serializable]
public class BaccaratPlayer
{
    public string UserId;
    public string DisplayName;
    public long Wallet;
    public int AvatarId;
    public int VipLevel;
    public bool IsOnline;
    public DateTime LastSeen;
    
    public BaccaratPlayer(string userId, string displayName, long wallet = 0, int avatarId = 0, int vipLevel = 0)
    {
        UserId = userId;
        DisplayName = displayName;
        Wallet = wallet;
        AvatarId = avatarId;
        VipLevel = vipLevel;
        IsOnline = true;
        LastSeen = DateTime.Now;
    }
}

public class BaccaratPlayerView : BasePlayerView
{
  
}
