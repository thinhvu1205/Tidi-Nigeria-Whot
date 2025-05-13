using SimpleJSON;
public class LastClaimRewardModel
{
    public PbLastClaimReward DataPbLastClaimReward = new();
}
public class PbLastClaimReward : IProto
{
    private const string 
        _LAST_CLAIM_UNIX = "lastClaimUnix",
        _NEXT_CLAIM_UNIX = "nextClaimUnix",
        _STREAK = "streak",
        _LAST_SPIN_NUMBER = "lastSpinNumber",
        _REACH_MAX_STREAK = "reachMaxStreak",
        _NUM_CLAIM = "numClaim";

    public long LastClaimUnix, NextClaimUnix, Streak, LastSpinNumber, NumClaim;
    public bool ReachMaxStreak;

    private void _Reset()
    {
        ReachMaxStreak = false;
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        LastClaimUnix = data[_LAST_CLAIM_UNIX].AsLong;
        NextClaimUnix = data[_NEXT_CLAIM_UNIX].AsLong;
        Streak = data[_STREAK].AsLong;
        LastSpinNumber = data[_LAST_SPIN_NUMBER].AsLong;
        ReachMaxStreak = data[_REACH_MAX_STREAK].AsBool;
        NumClaim = data[_NUM_CLAIM].AsLong;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_LAST_CLAIM_UNIX] = LastClaimUnix,
            [_NEXT_CLAIM_UNIX] = NextClaimUnix,
            [_STREAK] = Streak,
            [_LAST_SPIN_NUMBER] = LastSpinNumber,
            [_REACH_MAX_STREAK] = ReachMaxStreak,
            [_NUM_CLAIM] = NumClaim
        };
    }
}