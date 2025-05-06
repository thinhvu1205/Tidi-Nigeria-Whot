using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class QuickChatResponseModel
{
    public PbQuickChatResponse DataPbQuickChatResponse = new();
}
public class PbQuickChatResponse : IProto
{
    private const string 
        _TEXTS = "texts";
    public List<string> Texts;

    private void _Reset()
    {
        Texts = new();
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        foreach (JSONObject item in data[_TEXTS].AsArray)
        {
            Texts.Add(item);
        }
    }
    public JSONObject ParseToJSON()
    {
        JSONArray texts = new();
        foreach (string item in Texts) 
        {
            texts.Add(item);
        }
        return new()
        {
            [_TEXTS] = texts   
        };
    }
}
