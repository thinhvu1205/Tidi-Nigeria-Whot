using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class RegisterRequestModel
{
    public PbRegisterRequest DataPbRegisterRequest = new();
}
public class PbRegisterRequest : IProto
{
    private const string 
        _USERNAME = "userName", 
        _PASSWORD = "password"; 
    public string UserName, Password;

    private void _Reset()
    {
        UserName = Password = "";
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        UserName = data[_USERNAME].Value;
        Password = data[_PASSWORD].Value;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_USERNAME] = UserName,
            [_PASSWORD] = Password,
        };
    }
}
