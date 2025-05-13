using System.Collections.Generic;
using SimpleJSON;
public class UserGroupRequestModel
{
    public PbUserGroupRequest DataPbUserGroupRequest = new();
}
public class PbUserGroupRequest : IProto
{
    private const string 
        _LIMIT = "limit",
        _CUSOR = "cusor";
    public long Limit;
    public string Cusor;

    private void _Reset()
    {
        Cusor = "";
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        Limit = data[_LIMIT].AsLong;
        Cusor = data[_CUSOR].Value;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_LIMIT] = Limit,
            [_CUSOR] = Cusor
        };
    }
}
