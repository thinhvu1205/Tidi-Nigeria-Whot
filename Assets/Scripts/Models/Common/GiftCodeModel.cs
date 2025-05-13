using SimpleJSON;
public class GiftCodeModel
{
    public PbGiftCode DataPbGiftCode = new();
}
public class PbGiftCode : IProto
{
    private const string 
        _ID = "id",
        _CODE = "code",
        _N_CURRENT = "nCurrent",
        _N_MAX = "nMax",
        _VALUE = "value",
        _START_TIME_UNIX = "startTimeUnix",
        _END_TIME_UNIX = "endTimeUnix",
        _MESSAGE = "message",
        _VIP = "vip",
        _GIFT_CODE_TYPE = "giftCodeType",
        _REACH_MAX_CLAIM = "reachMaxClaim",
        _ALREADY_CLAIM = "alreadyClaim",
        _USER_ID = "userId",
        _OPEN_TO_CLAIM = "openToClaim",
        _ERR_CODE = "errCode";
    public long Id, NCurrent, NMax, Value, StartTimeUnix, EndTimeUnix, Vip;
    public string Code, Message, UserId;
    public int ErrCode;
    public bool ReachMaxClaim, AlreadyClaim, OpenToClaim;
    private EGiftCodeType GiftCodeType;

    private void _Reset()
    {
        Code = Message = UserId = "";
        ReachMaxClaim = AlreadyClaim = OpenToClaim = false;
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        Id = data[_ID].AsLong;
        Code = data[_CODE].Value;
        NCurrent = data[_N_CURRENT].AsLong;
        NMax = data[_N_MAX].AsLong;
        Value = data[_VALUE].AsLong;
        StartTimeUnix = data[_START_TIME_UNIX].AsLong;
        EndTimeUnix = data[_END_TIME_UNIX].AsLong;
        Message = data[_MESSAGE].Value;
        Vip = data[_VIP].AsLong;
        GiftCodeType = (EGiftCodeType) data[_GIFT_CODE_TYPE].AsInt;
        ReachMaxClaim = data[_REACH_MAX_CLAIM].AsBool;
        AlreadyClaim = data[_ALREADY_CLAIM].AsBool;
        UserId = data[_USER_ID].Value;
        OpenToClaim = data[_OPEN_TO_CLAIM].AsBool;
        ErrCode = data[_ERR_CODE].AsInt;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_ID] = Id,
            [_CODE] = Code,
            [_N_CURRENT] = NCurrent,
            [_N_MAX] = NMax,
            [_VALUE] = Value,
            [_START_TIME_UNIX] = StartTimeUnix,
            [_END_TIME_UNIX] = EndTimeUnix,
            [_MESSAGE] = Message,
            [_VIP] = Vip,
            [_GIFT_CODE_TYPE] = (int)GiftCodeType,
            [_REACH_MAX_CLAIM] = ReachMaxClaim,
            [_ALREADY_CLAIM] = AlreadyClaim,
            [_USER_ID] = UserId,
            [_OPEN_TO_CLAIM] = OpenToClaim,
            [_ERR_CODE] = ErrCode
        };
    }
}
