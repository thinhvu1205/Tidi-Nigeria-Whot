using SimpleJSON;
public class LeaderBoardRecordModel
{
    public PbLeaderBoardRecord DataPbLeaderBoardRecord = new();
}
public class PbLeaderBoardRecord : IProto
{
    private const string 
        _GAME_CODE = "gameCode",
        _USER_ID = "userId",
        _SCORE = "score",
        _CD_RESET_UNIX = "cdResetUnix";

    public string GameCode, UserId;
    public long Score, CdResetUnix;

    private void _Reset()
    {
        GameCode = UserId = "";
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        GameCode = data[_GAME_CODE].Value;
        UserId = data[_USER_ID].Value;
        Score = data[_SCORE].AsLong;
        CdResetUnix = data[_CD_RESET_UNIX].AsLong;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_GAME_CODE] = GameCode,
            [_USER_ID] = UserId,
            [_SCORE] = Score,
            [_CD_RESET_UNIX] = CdResetUnix
        };
    }
}