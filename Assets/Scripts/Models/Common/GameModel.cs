using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class GameModel
{
    public PbGame DataPbGame = new();
}
public class PbGame : IProto
{
    private const string 
        _CODE = "code",
        _ACTIVE = "active",
        _LOBBY_ID = "lobbyId",
        _LAYOUT = "layout",
        _ID = "id";
    public string Code, LobbyId;
    public bool Active;
    public PbLayout Layout;
    public long Id;

    private void _Reset()
    {
        Code = LobbyId = "";
        Active = false;
        Layout = new();
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        Code = data[_CODE].Value;
        Active = data[_ACTIVE].AsBool;
        LobbyId = data[_LOBBY_ID].Value;
        Layout = new PbLayout();
        Layout.ParseFromJSON(data[_LAYOUT].AsObject);
        Id = data[_ID].AsLong;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_CODE] = Code,
            [_ACTIVE] = Active,
            [_LOBBY_ID] = LobbyId,
            [_LAYOUT] = Layout.ParseToJSON(),
            [_ID] = Id
        };
    }
}
