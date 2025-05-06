using SimpleJSON;
public class BetListRequestModel
{
    public PbBetListRequest DataPbBetListRequest = new();
}
public class PbBetListRequest : IProto
{
    private const string 
        _CODE = "code";
    public string Code;

    private void _Reset()
    {
        Code = "";
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        Code = data[_CODE].Value;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_CODE] = Code
        };
    }
}
