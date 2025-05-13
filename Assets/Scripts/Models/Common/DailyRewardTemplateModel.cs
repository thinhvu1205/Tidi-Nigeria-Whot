using System.Collections.Generic;
using SimpleJSON;
public class DailyRewardTemplateModel
{
    public PbDailyRewardTemplate DataPbDailyRewardTemplate = new();
}
public class PbDailyRewardTemplate : IProto
{
    private const string 
        _REWARD_TEMPLATES = "rewardTemplates";
    public List<PbRewardTemplate> RewardTemplates;

    private void _Reset()
    {
        RewardTemplates = new();
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        foreach (JSONObject item in data[_REWARD_TEMPLATES].AsArray)
        {
            PbRewardTemplate game = new();
            game.ParseFromJSON(item);
            RewardTemplates.Add(game);
        }
    }
    public JSONObject ParseToJSON()
    {
        JSONArray rewardTemplates = new();
        foreach (var rewardTemplate in RewardTemplates)
        {
            rewardTemplates.Add(rewardTemplate.ParseToJSON());
        }

        return new()
        {
            [_REWARD_TEMPLATES] = rewardTemplates
        };
    }
}
