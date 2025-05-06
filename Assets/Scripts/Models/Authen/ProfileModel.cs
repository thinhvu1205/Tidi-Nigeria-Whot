using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class ProfileModel
{
    public PbProfile DataPbProfile = new();
}
public class PbProfile : IProto
{
    private const string 
        _USER_ID = "userId", 
        _USER_NAME = "userName",
        _DISPLAY_NAME = "displayName",
        _AVATAR_URL = "avatarUrl",
        _STATUS = "status",
        _ACCOUNT_CHIP = "accountChip",
        _BANK_CHIP = "bankChip",
        _REF_CODE = "refCode",
        _LANG_TAG = "langTag",
        _LINK_GROUP = "linkGroup",
        _LINK_FANPAGE_FB = "linkFanpageFb",
        _APP_CONFIG = "appConfig",
        _AVATAR_ID = "avatarId",
        _REGISTERABLE = "registrable",
        _VIP_LEVEL = "vipLevel",
        _LAST_ONLINE_TIME_UNIX = "lastOnlineTimeUnix",
        _CREATE_TIME_UNIX = "createTimeUnix",
        _REMAIN_TIME_INPUT_REF_CODE = "remainTimeInputRefCode",
        _LANG_AVAILABLES = "langAvailables",
        _PLAYING_MATCH = "playingMatch",
        _DEVICE_ID = "deviceId",
        _LAST_DEVICE_ID = "lastDeviceId",
        _REF_GAME = "refGame",
        _CURRENT_IP = "currentIp",
        _VIP_POINT = "vipPoint",
        _IS_ONLINE = "isOnline",
        _IS_BANNED = "isBanned",
        _LAST_LOGIN_UNIX = "lastLoginUnix",
        _USER_SID = "userSid";
    public string UserId, UserName, DisplayName, AvatarUrl, Status, RefCode, LangTag, LinkGroup, LinkFanpageFb, AppConfig, AvatarId,
    DeviceId, LastDeviceId, RefGame, CurrentIp;
    public long AccountChip, BankChip, VipLevel, LastOnlineTimeUnix, CreateTimeUnix, RemainTimeInputRefCode, VipPoint, LastLoginUnix, UserSid;
    public bool Registerable, IsOnline, IsBanned;
    public List<PbLangCode> LangAvailables;
    public PbPlayingMatch PlayingMatch;

    private void _Reset()
    {
        UserId = UserName = DisplayName = AvatarUrl = Status = RefCode = LangTag = LinkGroup = LinkFanpageFb = AppConfig = AvatarId =
        DeviceId = LastDeviceId = RefGame = CurrentIp = "";
        Registerable = IsOnline = IsBanned = false;
        LangAvailables = new();
        PlayingMatch = new();
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        UserId = data[_USER_ID].Value;
        UserName = data[_USER_NAME].Value;
        DisplayName = data[_DISPLAY_NAME].Value;
        AvatarUrl = data[_AVATAR_URL].Value;
        Status = data[_STATUS].Value;
        RefCode = data[_REF_CODE].Value;
        LangTag = data[_LANG_TAG].Value;
        LinkGroup = data[_LINK_GROUP].Value;
        LinkFanpageFb = data[_LINK_FANPAGE_FB].Value;
        AppConfig = data[_APP_CONFIG].Value;
        AvatarId = data[_AVATAR_ID].Value;
        DeviceId = data[_DEVICE_ID].Value;
        LastDeviceId = data[_LAST_DEVICE_ID].Value;
        RefGame = data[_REF_GAME].Value;
        CurrentIp = data[_CURRENT_IP].Value;
        AccountChip = data[_ACCOUNT_CHIP].AsLong;
        BankChip = data[_BANK_CHIP].AsLong;
        VipLevel = data[_VIP_LEVEL].AsLong;
        LastOnlineTimeUnix = data[_LAST_ONLINE_TIME_UNIX].AsLong;
        CreateTimeUnix = data[_CREATE_TIME_UNIX].AsLong;
        RemainTimeInputRefCode = data[_REMAIN_TIME_INPUT_REF_CODE].AsLong;
        VipPoint = data[_VIP_POINT].AsLong;
        LastLoginUnix = data[_LAST_LOGIN_UNIX].AsLong;
        UserSid = data[_USER_SID].AsLong;
        Registerable = data[_REGISTERABLE].AsBool;
        IsOnline = data[_IS_ONLINE].AsBool;
        IsBanned = data[_IS_BANNED].AsBool;
        foreach (JSONObject item in data[_LANG_AVAILABLES].AsArray)
        {
            PbLangCode plc = new();
            plc.ParseFromJSON(item);
            LangAvailables.Add(plc);
        }
        PbPlayingMatch pbPlayingMatch= new();
        pbPlayingMatch.ParseFromJSON(data[_PLAYING_MATCH].AsObject);
        PlayingMatch = pbPlayingMatch;
    }
    public JSONObject ParseToJSON()
    {
        JSONArray langCodes = new();
        foreach (PbLangCode item in LangAvailables) 
        {
            langCodes.Add(item.ParseToJSON());
        }
        return new()
        {
            [_USER_ID] = UserId,
            [_USER_NAME] = UserName,
            [_DISPLAY_NAME] = DisplayName,
            [_AVATAR_URL] = AvatarUrl,
            [_STATUS] = Status,
            [_ACCOUNT_CHIP] = AccountChip,
            [_BANK_CHIP] = BankChip,
            [_REF_CODE] = RefCode,
            [_LANG_TAG] = LangTag,
            [_LINK_GROUP] = LinkGroup,
            [_LINK_FANPAGE_FB] = LinkFanpageFb,
            [_APP_CONFIG] = AppConfig,
            [_AVATAR_ID] = AvatarId,
            [_REGISTERABLE] = Registerable,
            [_VIP_LEVEL] = VipLevel,
            [_LAST_ONLINE_TIME_UNIX] = LastOnlineTimeUnix,
            [_CREATE_TIME_UNIX] = CreateTimeUnix,
            [_REMAIN_TIME_INPUT_REF_CODE] = RemainTimeInputRefCode,
            [_LANG_AVAILABLES] = langCodes,
            [_PLAYING_MATCH] = PlayingMatch.ParseToJSON(),
            [_DEVICE_ID] = DeviceId,
            [_LAST_DEVICE_ID] = LastDeviceId,
            [_REF_GAME] = RefGame,
            [_CURRENT_IP] = CurrentIp,
            [_VIP_POINT] = VipPoint,
            [_IS_ONLINE] = IsOnline,
            [_IS_BANNED] = IsBanned,
            [_LAST_LOGIN_UNIX] = LastLoginUnix,
            [_USER_SID] = UserSid,
        };
    }
}
