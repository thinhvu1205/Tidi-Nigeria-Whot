using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class LangCodeModel
{
    public PbLangCode DataPLC = new();
}
public class PbLangCode : IProto
{
    private const string _ISOCODE = "isoCode", _DISPLAYNAME = "displayName", _SOURCEURL = "sourceUrl";
    public string IsoCode, DisplayName, SourceUrl;

    private void _Reset()
    {
        IsoCode = DisplayName = SourceUrl = "";
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        IsoCode = data[_ISOCODE].Value;
        DisplayName = data[_DISPLAYNAME].Value;
        SourceUrl = data[_SOURCEURL].Value;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_ISOCODE] = IsoCode,
            [_DISPLAYNAME] = DisplayName,
            [_SOURCEURL] = SourceUrl,
        };
    }
}
