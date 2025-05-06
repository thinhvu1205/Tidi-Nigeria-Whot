using System.Collections.Generic;
using SimpleJSON;
public class ListFreeChipModel
{
    public PbListFreeChip DataPbListFreeChip = new();
}
public class PbListFreeChip : IProto
{
    private const string 
        _FREECHIPS = "freechips",
        _NEXT_CUSOR = "nextCusor", 
        _PREV_CUSOR = "prevCusor",
        _TOTAL = "total",
        _OFFSET = "offset",
        _LIMIT = "limit";
    public List<PbFreeChip> Freechips = new();
    public string NextCusor, PrevCusor;
    public long Total, Offset, Limit;

    private void _Reset()
    {
        Freechips = new();
        NextCusor = PrevCusor = "";
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        Freechips = new List<PbFreeChip>();
        foreach (JSONObject item in data[_FREECHIPS].AsArray)
        {
            PbFreeChip chip = new();
            chip.ParseFromJSON(item);
            Freechips.Add(chip);
        }

        NextCusor = data[_NEXT_CUSOR].Value;
        PrevCusor = data[_PREV_CUSOR].Value;
        Total = data[_TOTAL].AsLong;
        Offset = data[_OFFSET].AsLong;
        Limit = data[_LIMIT].AsLong;
    }
    public JSONObject ParseToJSON()
    {
        JSONArray freeChips = new();
        foreach (PbFreeChip chip in Freechips)
        {
            freeChips.Add(chip.ParseToJSON());
        }
        return new()
        {
            [_FREECHIPS] = freeChips,
            [_NEXT_CUSOR] = NextCusor,
            [_PREV_CUSOR] = PrevCusor,
            [_TOTAL] = Total,
            [_OFFSET] = Offset,
            [_LIMIT] = Limit
        };
    }
}
