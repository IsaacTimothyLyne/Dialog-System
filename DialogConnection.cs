[System.Serializable]
public class DialogConnection
{
    public string FromNodeId;
    public string ToNodeId;

    public DialogConnection(string fromNodeId, string toNodeId)
    {
        FromNodeId = fromNodeId;
        ToNodeId = toNodeId;
    }
}
