public class ConnectionPoint
{
    public enum ConnectionPointType
    {
        DialogNode,
        OptionNode
    }

    public string Id;
    public ConnectionPointType Type;

    public ConnectionPoint(string id, ConnectionPointType type)
    {
        Id = id;
        Type = type;
    }
}
