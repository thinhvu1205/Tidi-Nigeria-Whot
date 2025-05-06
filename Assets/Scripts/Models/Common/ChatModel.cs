using SimpleJSON;
public class ChatModel
{
    public PbChat DataPbChat = new();
}
public class PbChat : IProto
{
    private const string 
        _TYPE_CHAT = "typeChat",
        _ID = "id",
        _CONTENT = "content";
    public ETypeChat TypeChat;
    public string Id, Content;

    private void _Reset()
    {
        Id = Content = "";
    }
    public void ParseFromJSON(JSONObject data)
    {
        _Reset();
        TypeChat = (ETypeChat) data[_TYPE_CHAT].AsInt;
        Id = data[_ID].Value;
        Content = data[_CONTENT].Value;
    }
    public JSONObject ParseToJSON()
    {
        return new()
        {
            [_TYPE_CHAT] = (int) TypeChat,
            [_ID] = Id,
            [_CONTENT] = Content
        };
    }
}
