using System.Collections.Generic;
using SimpleJSON;
public class ListExchangeInfoModelModel
{
    public PbListExchangeInfoModel DataPbListExchangeInfoModel = new();
}
public class PbListExchangeInfoModel : IProto
{
    private const string 
        _EXCHANGE_INFOS = "exchangeInfos",
        _NEXT_CUSOR = "nextCusor",
        _PREV_CUSOR = "prevCusor",
        _TOTAL = "total",
        _OFFSET = "offset",
        _LIMIT = "limit",
        _FROM = "from",
        _TO = "to";

    public List<PbExchangeInfo> ExchangeInfos = new();
    public string NextCusor, PrevCusor;
    public long Total, Offset, Limit, From, To;

    private void _Reset()
    {
        ExchangeInfos = new();
        NextCusor = PrevCusor = "";
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        ExchangeInfos = new();
        foreach (JSONObject item in data[_EXCHANGE_INFOS].AsArray)
        {
            PbExchangeInfo exchangeInfo = new();
            exchangeInfo.ParseFromJSON(item);
            ExchangeInfos.Add(exchangeInfo);
        }

        NextCusor = data[_NEXT_CUSOR].Value;
        PrevCusor = data[_PREV_CUSOR].Value;
        Total = data[_TOTAL].AsLong;
        Offset = data[_OFFSET].AsLong;
        Limit = data[_LIMIT].AsLong;
        From = data[_FROM].AsLong;
        To = data[_TO].AsLong;
    }
    public JSONObject ParseToJSON()
    {
        JSONArray exchangeInfos = new();
        foreach (PbExchangeInfo chip in ExchangeInfos)
        {
            exchangeInfos.Add(chip.ParseToJSON());
        }
        return new()
        {
            [_EXCHANGE_INFOS] = exchangeInfos,
            [_NEXT_CUSOR] = NextCusor,
            [_PREV_CUSOR] = PrevCusor,
            [_TOTAL] = Total,
            [_OFFSET] = Offset,
            [_LIMIT] = Limit,
            [_FROM] = From,
            [_TO] = To
        };
    }
}