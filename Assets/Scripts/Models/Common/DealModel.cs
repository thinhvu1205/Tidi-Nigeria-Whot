using SimpleJSON;
public class DealModel
{
    public PbDeal DataPbDeal = new();
}
public class PbDeal : IProto
{
    private const string 
        _ID = "id",
        _CHIPS = "chips",
        _AMOUNT_CHIPS = "amountChips",
        _BONUS = "bonus",
        _PRICE = "price",
        _NAME = "name",
        _CURRENCY = "currency",
        _PERCENT = "percent",
        _CHIP_PER_UNIT = "chipPerUnit";

    public string Id, Price, Name, Currency, Percent;
    public long Chips, AmountChips, Bonus, ChipPerUnit;

    private void _Reset()
    {
        Id = Price = Name = Currency = Percent = "";
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
         Id = data[_ID].Value;
        Chips = data[_CHIPS].AsLong;
        AmountChips = data[_AMOUNT_CHIPS].AsLong;
        Bonus = data[_BONUS].AsLong;
        Price = data[_PRICE].Value;
        Name = data[_NAME].Value;
        Currency = data[_CURRENCY].Value;
        Percent = data[_PERCENT].Value;
        ChipPerUnit = data[_CHIP_PER_UNIT].AsLong;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_ID] = Id,
            [_CHIPS] = Chips,
            [_AMOUNT_CHIPS] = AmountChips,
            [_BONUS] = Bonus,
            [_PRICE] = Price,
            [_NAME] = Name,
            [_CURRENCY] = Currency,
            [_PERCENT] = Percent,
            [_CHIP_PER_UNIT] = ChipPerUnit
        };
    }
}