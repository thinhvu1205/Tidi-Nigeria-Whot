using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class ChangePasswordRequestModel
{
    public PbChangePasswordRequest DataPbChangePassword = new();
}
public class PbChangePasswordRequest : IProto
{
    private const string 
        _OLDPASSWORD = "oldPassword", 
        _PASSWORD = "password"; 
    public string OldPassword, Password;

    private void _Reset()
    {
        OldPassword = Password = "";
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        OldPassword = data[_OLDPASSWORD].Value;
        Password = data[_PASSWORD].Value;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_OLDPASSWORD] = OldPassword,
            [_PASSWORD] = Password,
        };
    }
}
