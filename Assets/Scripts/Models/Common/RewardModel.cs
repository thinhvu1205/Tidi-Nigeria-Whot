using SimpleJSON;
public class RewardModel
{
    public PbReward DataPbReward = new();
}
public class PbReward : IProto
{
    private const string 
        _BASIC_CHIP = "basicChip",
        _PERCENT_BONUS = "percentBonus",
        _BONUS_CHIP = "bonusChip",
        _ONLINE_CHIP = "onlineChip",
        _TOTAL_CHIP = "totalChip",
        _STREAK = "streak",
        _ONLINE_SEC = "onlineSec",
        _CAN_CLAIM = "canClaim",
        _NUM_CLAIM = "numClaim",
        _LAST_CLAIM_UNIX = "lastClaimUnix",
        _NEXT_CLAIM_UNIX = "nextClaimUnix",
        _NEXT_CLAIM_SEC = "nextClaimSec",
        _REACH_MAX_STREAK = "reachMaxStreak",
        _LAST_SPIN_NUMBER = "lastSpinNumber",
        _LAST_ONLINE_UNIX = "lastOnlineUnix";

    public long BasicChip, BonusChip, OnlineChip, TotalChip, Streak, OnlineSec, NumClaim, LastClaimUnix, NextClaimUnix, NextClaimSec, LastSpinNumber, LastOnlineUnix;
    public float PercentBonus;
    public bool CanClaim, ReachMaxStreak;

    private void _Reset()
    {
        CanClaim = ReachMaxStreak = false;
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        BasicChip = data[_BASIC_CHIP].AsLong;
        PercentBonus = data[_PERCENT_BONUS].AsFloat;
        BonusChip = data[_BONUS_CHIP].AsLong;
        OnlineChip = data[_ONLINE_CHIP].AsLong;
        TotalChip = data[_TOTAL_CHIP].AsLong;
        Streak = data[_STREAK].AsLong;
        OnlineSec = data[_ONLINE_SEC].AsLong;
        CanClaim = data[_CAN_CLAIM].AsBool;
        NumClaim = data[_NUM_CLAIM].AsLong;
        LastClaimUnix = data[_LAST_CLAIM_UNIX].AsLong;
        NextClaimUnix = data[_NEXT_CLAIM_UNIX].AsLong;
        NextClaimSec = data[_NEXT_CLAIM_SEC].AsLong;
        ReachMaxStreak = data[_REACH_MAX_STREAK].AsBool;
        LastSpinNumber = data[_LAST_SPIN_NUMBER].AsLong;
        LastOnlineUnix = data[_LAST_ONLINE_UNIX].AsLong;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_BASIC_CHIP] = BasicChip,
            [_PERCENT_BONUS] = PercentBonus,
            [_BONUS_CHIP] = BonusChip,
            [_ONLINE_CHIP] = OnlineChip,
            [_TOTAL_CHIP] = TotalChip,
            [_STREAK] = Streak,
            [_ONLINE_SEC] = OnlineSec,
            [_CAN_CLAIM] = CanClaim,
            [_NUM_CLAIM] = NumClaim,
            [_LAST_CLAIM_UNIX] = LastClaimUnix,
            [_NEXT_CLAIM_UNIX] = NextClaimUnix,
            [_NEXT_CLAIM_SEC] = NextClaimSec,
            [_REACH_MAX_STREAK] = ReachMaxStreak,
            [_LAST_SPIN_NUMBER] = LastSpinNumber,
            [_LAST_ONLINE_UNIX] = LastOnlineUnix
        };
    }
}