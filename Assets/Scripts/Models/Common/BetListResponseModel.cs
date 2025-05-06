using System.Collections.Generic;
using SimpleJSON;
public class BetListResponseModel
{
    public PbBetListResponse DataPbBetListResponse = new();
}
public class PbBetListResponse : IProto
{
    private const string 
        _BETS = "bets";
    public List<int> Bets;

    private void _Reset()
    {
        Bets = new();
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        foreach (JSONObject item in data[_BETS].AsArray)
        {
            Bets.Add(item);
        }    
    }

    public JSONObject ParseToJSON()
    {
        JSONArray bets = new();
        foreach (int item in Bets) 
        {
            bets.Add(item);
        }
        return new()
        {
            [_BETS] = bets   
        };
    }
}
