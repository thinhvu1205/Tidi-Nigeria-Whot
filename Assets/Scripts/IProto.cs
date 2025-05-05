using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public interface IProto
{
    public JSONObject ParseToJSON();
    public void ParseFromJSON(JSONObject data);
}
