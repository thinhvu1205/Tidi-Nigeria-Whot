using SimpleJSON;
public class FreeChipModel
{
    public PbFreeChip DataPbFreeChip = new();
}
public class PbFreeChip : IProto
{
    private const string 
        _ID = "id",
        _SENDER_ID = "senderId",
        _RECIPIENT_ID = "recipientId",
        _TITLE = "title",
        _CONTENT = "content",
        _CHIPS = "chips",
        _CLAIMABLE = "claimable",
        _ACTION = "action",
        _CLAIM_TIME_UNIX = "claimTimeUnix",
        _CLAIM_STATUS = "claimStaus";

    public long Id, Chips, ClaimTimeUnix;
    public string SenderId, RecipientId, Title, Content, Action;
    public bool Claimable;
    public EClaimStatus ClaimStatus;

    private void _Reset()
    {
        SenderId = RecipientId = Title = Content = Action;
        Claimable = false;
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        Id = data[_ID].AsLong;
        SenderId = data[_SENDER_ID].Value;
        RecipientId = data[_RECIPIENT_ID].Value;
        Title = data[_TITLE].Value;
        Content = data[_CONTENT].Value;
        Chips = data[_CHIPS].AsLong;
        Claimable = data[_CLAIMABLE].AsBool;
        Action = data[_ACTION].Value;
        ClaimTimeUnix = data[_CLAIM_TIME_UNIX].AsLong;

        ClaimStatus = (EClaimStatus) data[_CLAIM_STATUS].AsInt;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_ID] = Id,
            [_SENDER_ID] = SenderId,
            [_RECIPIENT_ID] = RecipientId,
            [_TITLE] = Title,
            [_CONTENT] = Content,
            [_CHIPS] = Chips,
            [_CLAIMABLE] = Claimable,
            [_ACTION] = Action,
            [_CLAIM_TIME_UNIX] = ClaimTimeUnix,
            [_CLAIM_STATUS] = (int) ClaimStatus
        };
    }
}

public enum EClaimStatus {
    CLAIM_STATUS_UNSPECIFIED = 0,
    CLAIM_STATUS_WAIT_ADMIN_ACCEPT = 1,
    CLAIM_STATUS_WAIT_USER_CLAIM = 2,
    CLAIM_STATUS_CLAIMED = 3,
    CLAIM_STATUS_REJECT = 4
}