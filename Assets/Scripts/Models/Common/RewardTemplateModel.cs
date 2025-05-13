using System.Collections.Generic;
using SimpleJSON;
public class RewardTemplateModel
{
    public PbRewardTemplate DataPbRewardTemplate = new();
}
public class PbRewardTemplate : IProto
{
    private const string 
        _BASIC_CHIPS = "basicChips",
        _PERCEN_BONUS = "percenBonus",
        _ONLINE_SEC = "onlineSec",
        _ONLINE_CHIP = "onlineChip",
        _STREAK = "streak";
    public List<long> BasicChips = new();
    public float PercenBonus;
    public long OnlineSec, OnlineChip, Streak;

    private void _Reset()
    {
        BasicChips = new();
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        BasicChips = new List<long>();
        foreach (JSONObject val in data[_BASIC_CHIPS].AsArray)
        {
            BasicChips.Add(val);
        }

        PercenBonus = data[_PERCEN_BONUS].AsFloat;
        OnlineSec = data[_ONLINE_SEC].AsLong;
        OnlineChip = data[_ONLINE_CHIP].AsLong;
        Streak = data[_STREAK].AsLong;
    }
    public JSONObject ParseToJSON()
    {
        JSONArray basicChips = new();
        foreach (long chip in BasicChips)
        {
            basicChips.Add(chip);
        }
        return new()
        {
            [_BASIC_CHIPS] = basicChips,
            [_PERCEN_BONUS] = PercenBonus,
            [_ONLINE_SEC] = OnlineSec,
            [_ONLINE_CHIP] = OnlineChip,
            [_STREAK] = Streak
        };
    }
}
