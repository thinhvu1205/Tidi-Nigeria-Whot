using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class SimpleProfileModel
{
    public PbSimpleProfile DataPbSimpleProfile = new();
}
public class PbSimpleProfile : IProto
{
    private const string 
        _USER_ID = "userId",
        _USER_NAME = "userName",
        _DISPLAY_NAME = "displayName",
        _STATUS = "status",
        _ACCOUNT_CHIP = "accountChip",
        _AVATAR_ID = "avatarId",
        _VIP_LEVEL = "vipLevel",
        _PLAYING_MATCH = "playingMatch",
        _USER_SID = "userSid";
    public string UserId, UserName, DisplayName, Status, AvatarId;
    public long AccountChip, VipLevel, UserSid;
    public PbPlayingMatch PlayingMatch;

    private void _Reset()
    {
        UserId = UserName = DisplayName = Status = AvatarId = "";
        PlayingMatch = new();
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        UserId = data[_USER_ID].Value;
        UserName = data[_USER_NAME].Value;
        DisplayName = data[_DISPLAY_NAME].Value;
        Status = data[_STATUS].Value;
        AvatarId = data[AvatarId].Value;
        AccountChip = data[_ACCOUNT_CHIP].AsLong;
        VipLevel = data[_VIP_LEVEL].AsLong;
        UserSid = data[_USER_SID].AsLong;

        PbPlayingMatch pbPlayingMatch= new();
        pbPlayingMatch.ParseFromJSON(data[_PLAYING_MATCH].AsObject);
        PlayingMatch = pbPlayingMatch;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_USER_ID] = UserId,
            [_USER_NAME] = UserName,
            [_DISPLAY_NAME] = DisplayName,
            [_STATUS] = Status,
            [_ACCOUNT_CHIP] = AccountChip,
            [_AVATAR_ID] = AvatarId,
            [_VIP_LEVEL] = VipLevel,
            [_PLAYING_MATCH] = PlayingMatch.ParseToJSON(),
            [_USER_SID] = UserSid,
        };
    }
}
