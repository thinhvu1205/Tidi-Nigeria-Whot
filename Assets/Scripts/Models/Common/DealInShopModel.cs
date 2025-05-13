using System.Collections.Generic;
using SimpleJSON;
public class DealInShopModel
{
    public PbDealInShop DataPbDealInShop = new();
}
public class PbDealInShop : IProto
{
    private const string 
        _BEST = "best",
        _IAPS = "iaps",
        _GCASHES = "gcashes",
        _SMS = "sms";

    public PbDeal Best;
    public List<PbDeal> Iaps = new();
    public List<PbDeal> Gcashes = new();
    public List<PbDeal> Sms = new();

    private void _Reset()
    {
        Best = new();
        Iaps = new();
        Gcashes = new();
        Sms = new();
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        Best = new PbDeal();
        Best.ParseFromJSON(data[_BEST].AsObject);

        Iaps = new List<PbDeal>();
        foreach (JSONObject item in data[_IAPS].AsArray)
        {
            PbDeal deal = new();
            deal.ParseFromJSON(item);
            Iaps.Add(deal);
        }

        Gcashes = new List<PbDeal>();
        foreach (JSONObject item in data[_GCASHES].AsArray)
        {
            PbDeal deal = new();
            deal.ParseFromJSON(item);
            Gcashes.Add(deal);
        }

        Sms = new List<PbDeal>();
        foreach (JSONObject item in data[_SMS].AsArray)
        {
            PbDeal deal = new();
            deal.ParseFromJSON(item);
            Sms.Add(deal);
        }
    }
    public JSONObject ParseToJSON()
    {
        JSONArray iaps = new();
        foreach (PbDeal deal in Iaps)
        {
            iaps.Add(deal.ParseToJSON());
        }
        JSONArray gcashes = new();
        foreach (PbDeal deal in Gcashes)
        {
            gcashes.Add(deal.ParseToJSON());
        }
        JSONArray sms = new();
        foreach (PbDeal deal in Sms)
        {
            sms.Add(deal.ParseToJSON());
        }
        return new()
        {
            [_BEST] = Best.ParseToJSON(),
            [_GCASHES] = gcashes,
            [_SMS] = sms,
            [_IAPS] = iaps
        };
    }
}