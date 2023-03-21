using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogTimelineAsset", menuName = "Dialog System/Dialog Timeline Asset")]

public class DialogTimelineAsset : ScriptableObject
{
    public List<DialogNode> Nodes; // A list of all dialog nodes in the timeline
    public List<OptionNode> OptionNodes;

    public int MaxOptionsCount = 4;

    public enum LayoutDirection
    {
        Horizontal,
        Vertical
    }

    [SerializeField]
    private LayoutDirection optionsLayoutDirection = LayoutDirection.Vertical;

    public LayoutDirection OptionsLayoutDirection
    {
        get { return optionsLayoutDirection; }
        set { optionsLayoutDirection = value; }
    }

    public DialogTimelineAsset()
    {
        Nodes = new List<DialogNode>();
    }

    public bool TryGetNodeById(string id, out DialogNode node)
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            if (Nodes[i].Id == id)
            {
                node = Nodes[i];
                return true;
            }
        }

        node = null;
        return false;
    }


    public DialogNode GetNodeById(string nodeId)
    {
        return Nodes.Find(node => node.Id == nodeId);
    }
}
