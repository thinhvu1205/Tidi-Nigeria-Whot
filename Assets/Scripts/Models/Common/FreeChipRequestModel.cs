using SimpleJSON;
public class FreeChipRequestModel
{
    public PbFreeChipRequest DataPbFreeChipRequest = new();
}
public class PbFreeChipRequest : IProto
{
    private const string 
        _USER_ID = "userId",
        _LIMIT = "limit",
        _CUSOR = "cusor", 
        _CLAIM_STATUS = "claimStaus";

    public string UserId, Cusor;
    public long Limit;
    public EClaimStatus ClaimStatus;

    private void _Reset()
    {
        UserId = Cusor = "";
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        UserId = data[_USER_ID].Value;
        Limit = data[_LIMIT].AsLong;
        Cusor = data[_CUSOR].Value;
        ClaimStatus = (EClaimStatus) data[_CLAIM_STATUS].AsInt;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_USER_ID] = UserId,
            [_LIMIT] = Limit,
            [_CUSOR] = Cusor,
            [_CLAIM_STATUS] = (int) ClaimStatus
        };
    }
}