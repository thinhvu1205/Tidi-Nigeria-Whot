using System.Collections.Generic;
using SimpleJSON;
public class UserGroupModel
{
    public PbUserGroup DataPbUserGroup = new();
}
public class PbUserGroup : IProto
{
    private const string 
        _ID = "id",
        _NAME = "name";
    public long Id;
    public string Name;

    private void _Reset()
    {
        Name = "";
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        Id = data[_ID].AsLong;
        Name = data[_NAME].Value;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_ID] = Id,
            [_NAME] = Name
        };
    }
}
