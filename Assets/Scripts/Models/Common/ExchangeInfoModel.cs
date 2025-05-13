using SimpleJSON;
public class ExchangeInfoModel
{
    public PbExchangeInfo DataPbExchangeInfo = new();
}
public class PbExchangeInfo : IProto
{
    private const string 
        _ID = "id",
        _ID_DEAL = "idDeal",
        _CHIPS = "chips",
        _PRICE = "price",
        _STATUS = "status",
        _UNLOCK = "unlock",
        _CASH_ID = "cashId",
        _CASH_TYPE = "cashType",
        _USER_ID_REQUEST = "userIdRequest",
        _USER_NAME_REQUEST = "userNameRequest",
        _VIP_LV = "vipLv",
        _DEVICE_ID = "deviceId",
        _USER_ID_HANDLING = "userIdHandling",
        _USER_NAME_HANDLING = "userNameHandling",
        _REASON = "reason",
        _CURSOR = "cursor",
        _CREATE_TIME = "createTime";

    public string Id, IdDeal, Price, CashId, CashType, UserIdRequest, UserNameRequest, DeviceId, UserIdHandling, UserNameHandling, Reason, Cursor;
    public long Chips, Status, VipLv, CreateTime;
    public int Unlock;

    private void _Reset()
    {
        Id = IdDeal = Price = CashId = CashType = UserIdRequest = UserNameRequest = DeviceId = UserIdHandling = UserNameHandling = Reason = Cursor = "";
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        Id = data[_ID].Value;
        IdDeal = data[_ID_DEAL].Value;
        Chips = data[_CHIPS].AsLong;
        Price = data[_PRICE].Value;
        Status = data[_STATUS].AsLong;
        Unlock = data[_UNLOCK].AsInt;
        CashId = data[_CASH_ID].Value;
        CashType = data[_CASH_TYPE].Value;
        UserIdRequest = data[_USER_ID_REQUEST].Value;
        UserNameRequest = data[_USER_NAME_REQUEST].Value;
        VipLv = data[_VIP_LV].AsLong;
        DeviceId = data[_DEVICE_ID].Value;
        UserIdHandling = data[_USER_ID_HANDLING].Value;
        UserNameHandling = data[_USER_NAME_HANDLING].Value;
        Reason = data[_REASON].Value;
        Cursor = data[_CURSOR].Value;
        CreateTime = data[_CREATE_TIME].AsLong;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_ID] = Id,
            [_ID_DEAL] = IdDeal,
            [_CHIPS] = Chips,
            [_PRICE] = Price,
            [_STATUS] = Status,
            [_UNLOCK] = Unlock,
            [_CASH_ID] = CashId,
            [_CASH_TYPE] = CashType,
            [_USER_ID_REQUEST] = UserIdRequest,
            [_USER_NAME_REQUEST] = UserNameRequest,
            [_VIP_LV] = VipLv,
            [_DEVICE_ID] = DeviceId,
            [_USER_ID_HANDLING] = UserIdHandling,
            [_USER_NAME_HANDLING] = UserNameHandling,
            [_REASON] = Reason,
            [_CURSOR] = Cursor,
            [_CREATE_TIME] = CreateTime
        };
    }
}