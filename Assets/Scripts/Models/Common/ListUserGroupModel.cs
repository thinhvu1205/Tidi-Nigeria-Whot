using System.Collections.Generic;
using SimpleJSON;
public class ListUserGroupModel
{
    public PbListUserGroup DataPbListUserGroup = new();
}
public class PbListUserGroup : IProto
{
    private const string 
        _USER_GROUPS = "userGroups",
        _NEXT_CUSOR = "nextCusor",
        _PREV_CUSOR = "prevCusor",
        _TOTAL = "total",
        _OFFSET = "offset",
        _LIMIT = "limit";
    public List<PbUserGroup> UserGroups = new();
    public string NextCusor, PrevCusor;
    public long Total, Offset, Limit;

    private void _Reset()
    {
        UserGroups = new();
        NextCusor = PrevCusor = "";
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        foreach (JSONObject item in data[_USER_GROUPS].AsArray)
        {
            PbUserGroup userGroup = new();
            userGroup.ParseFromJSON(item);
            UserGroups.Add(userGroup);
        }
        NextCusor = data[_NEXT_CUSOR].Value;
        PrevCusor = data[_PREV_CUSOR].Value;
        Total = data[_TOTAL].AsLong;
        Offset = data[_OFFSET].AsLong;
        Limit = data[_LIMIT].AsLong;
    }
    public JSONObject ParseToJSON()
    {
        JSONArray userGroups = new();
        foreach (PbUserGroup userGroup in UserGroups)
        {
            userGroups.Add(userGroup.ParseToJSON());
        }
        return new()
        {
            [_USER_GROUPS] = userGroups,
            [_NEXT_CUSOR] = NextCusor,
            [_PREV_CUSOR] = PrevCusor,
            [_TOTAL] = Total,
            [_OFFSET] = Offset,
            [_LIMIT] = Limit
        };
    }
}
