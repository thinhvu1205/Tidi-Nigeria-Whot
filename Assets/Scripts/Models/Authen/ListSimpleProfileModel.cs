using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class ListSimpleProfileModel
{
    public PbListSimpleProfile DataPbListSimpleProfile = new();
}
public class PbListSimpleProfile : IProto
{
    private const string 
        _PROFILES = "profiles";
    public List<PbSimpleProfile> Profiles;

    private void _Reset()
    {
        Profiles = new();
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        foreach (JSONObject item in data[_PROFILES].AsArray)
        {
            PbSimpleProfile pProfile = new();
            pProfile.ParseFromJSON(item);
            Profiles.Add(pProfile);
        }
    }
    public JSONObject ParseToJSON()
    {
        JSONArray profiles = new();
        foreach (PbSimpleProfile item in Profiles) 
        {
            profiles.Add(item.ParseToJSON());
        }
        return new()
        {
            [_PROFILES] = profiles   
        };
    }
}
