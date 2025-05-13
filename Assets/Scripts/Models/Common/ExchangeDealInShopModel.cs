using System.Collections.Generic;
using SimpleJSON;
public class ExchangeDealInShopModel
{
    public PbExchangeDealInShop DataPbExchangeDealInShop = new();
}
public class PbExchangeDealInShop : IProto
{
    private const string 
        _GCASHES = "gcashes";

    public List<PbDeal> Gcashes = new();

    private void _Reset()
    {
        Gcashes = new();
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();

        Gcashes = new List<PbDeal>();
        foreach (JSONObject item in data[_GCASHES].AsArray)
        {
            PbDeal deal = new();
            deal.ParseFromJSON(item);
            Gcashes.Add(deal);
        }
    }
    public JSONObject ParseToJSON()
    {
        JSONArray gcashes = new();
        foreach (PbDeal deal in Gcashes)
        {
            gcashes.Add(deal.ParseToJSON());
        }

        return new()
        {
            [_GCASHES] = gcashes,
        };
    }
}