using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class GameListResponseModel
{
    public PbGameListResponse DataPbGameListResponse = new();
}
public class PbGameListResponse : IProto
{
    private const string _GAMES = "games";
    public List<PbGame> Games = new();

    private void _Reset()
    {
        Games = new();
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        foreach (JSONObject item in data[_GAMES].AsArray)
        {
            PbGame game = new();
            game.ParseFromJSON(item);
            Games.Add(game);
        }
    }
    public JSONObject ParseToJSON()
    {
        JSONArray gameList = new();
        foreach (var game in Games)
        {
            gameList.Add(game.ParseToJSON());
        }

        return new()
        {
            [_GAMES] = gameList
        };
    }
}
