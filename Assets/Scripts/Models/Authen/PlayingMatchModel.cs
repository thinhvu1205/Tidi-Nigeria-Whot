using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class PlayingMatchModel
{
    public PbPlayingMatch DataPbPlayingMatch = new();
}
public class PbPlayingMatch : IProto
{
    private const string 
        _CODE = "code",
        _MATCH_ID = "matchId",
        _LEAVE_TIME = "leaveTime",
        _MCB = "mcb",
        _BET = "bet";
 
    public string Code, MatchId;
    public long LeaveTime, Mcb, Bet;

    private void _Reset()
    {
        Code = MatchId = "";
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        Code = data[_CODE].Value;
        MatchId = data[_MATCH_ID].Value;
        LeaveTime = data[_LEAVE_TIME].AsLong;
        Mcb = data[_MCB].AsLong;
        Bet = data[_BET].AsLong;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_CODE] = Code,
            [_MATCH_ID] = MatchId,
            [_LEAVE_TIME] = LeaveTime,
            [_MCB] = Mcb,
            [_BET] = _BET,
        };
    }
}
