using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogTimelineAsset", menuName = "Dialog System/Dialog Timeline Asset", order = 0)]
public class DialogTimelineAsset : ScriptableObject
{
    public List<DialogNode> DialogNodes;
    public List<OptionNode> OptionNodes;

    private void OnEnable()
    {
        if (DialogNodes == null)
        {
            DialogNodes = new List<DialogNode>();
        }

        if (OptionNodes == null)
        {
            OptionNodes = new List<OptionNode>();
        }
    }
    public List<OptionNode> GetConnectedOptionNodes(DialogNode dialogNode)
    {
        List<OptionNode> connectedOptionNodes = new List<OptionNode>();

        foreach (var connection in DialogNodeConnections)
        {
            if (connection.FromNodeId == dialogNode.Id)
            {
                OptionNode optionNode = OptionNodes.Find(x => x.Id == connection.ToNodeId);
                if (optionNode != null)
                {
                    connectedOptionNodes.Add(optionNode);
                }
            }
        }

        return connectedOptionNodes;
    }

}
