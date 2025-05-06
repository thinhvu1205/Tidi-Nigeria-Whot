using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class ListProfileModel
{
    public PbListProfile DataPbListProfile = new();
}
public class PbListProfile : IProto
{
    private const string 
        _PROFILES = "profiles";
    public List<PbProfile> Profiles;

    private void _Reset()
    {
        Profiles = new();
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        foreach (JSONObject item in data[_PROFILES].AsArray)
        {
            PbProfile pProfile = new();
            pProfile.ParseFromJSON(item);
            Profiles.Add(pProfile);
        }
    }
    public JSONObject ParseToJSON()
    {
        JSONArray profiles = new();
        foreach (PbProfile item in Profiles) 
        {
            profiles.Add(item.ParseToJSON());
        }
        return new()
        {
            [_PROFILES] = profiles   
        };
    }
}
