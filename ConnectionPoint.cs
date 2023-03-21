public class ConnectionPoint
{
    public enum ConnectionPointType
    {
        DialogNode,
        EndNode,
        StartNode,
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
