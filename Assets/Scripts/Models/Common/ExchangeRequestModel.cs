using SimpleJSON;
public class ExchangeRequestModel
{
    public PbExchangeRequest DataPbExchangeRequest = new();
}
public class PbExchangeRequest : IProto
{
    private const string 
        _ID = "id",
        _USER_ID_REQUEST = "userIdRequest",
        _LIMIT = "limit",
        _FROM = "from",
        _TO = "to",
        _CUSOR = "cusor",
        _CASH_TYPE = "cashType";

    public string Id, UserIdRequest, Cusor, CashType;
    public long Limit, From, To;

    private void _Reset()
    {
        Id = UserIdRequest = Cusor = CashType = "";
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        Id = data[_ID].Value;
        UserIdRequest = data[_USER_ID_REQUEST].Value;
        Limit = data[_LIMIT].AsLong;
        From = data[_FROM].AsLong;
        To = data[_TO].AsLong;
        Cusor = data[_CUSOR].Value;
        CashType = data[_CASH_TYPE].Value;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_ID] = Id,
            [_USER_ID_REQUEST] = UserIdRequest,
            [_LIMIT] = Limit,
            [_FROM] = From,
            [_TO] = To,
            [_CUSOR] = Cusor,
            [_CASH_TYPE] = CashType
        };
    }
}