using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogTimelineAsset", menuName = "Dialog System/Dialog Timeline Asset")]

public class DialogTimelineAsset : ScriptableObject
{
    [SerializeField] private List<DialogNode> DialogNodes; // A list of all dialog nodes in the timeline
    [SerializeField] private List<OptionNode> OptionNodes;


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

    public int GetOptionNodeCount()
    {
        return OptionNodes.Count;
    }

    public int GetDialogNodeCount()
    {
        return DialogNodes.Count;
    }

    public void AddDialogNode(DialogNode node)
    {
        DialogNodes.Add(node);
    }

    public void RemoveDialogNode(int index)
    {
        DialogNodes.RemoveAt(index);
    }

    public void AddOptionNode(OptionNode optionNode)
    {
        OptionNodes.Add(optionNode);
    }

    public void RemoveOptionNode(int index)
    {
        OptionNodes.RemoveAt(index);
    }

    public DialogTimelineAsset()
    {
        DialogNodes = new List<DialogNode>();
        OptionNodes = new List<OptionNode>();
    }

    public bool TryGetOptionNodeById(string id, out OptionNode node)
    {
        for (int i = 0; i < OptionNodes.Count; i++)
        {
            if (OptionNodes[i].Id == id)
            {
                node = OptionNodes[i];
                return true;
            }
        }

        node = null;
        return false;
    }


    public OptionNode GetOptionNodeById(string nodeId)
    {
        return OptionNodes.Find(node => node.Id == nodeId);
    }

    public bool TryGetDialogNodeById(string id, out DialogNode node)
    {
        for (int i = 0; i < DialogNodes.Count; i++)
        {
            if (DialogNodes[i].Id == id)
            {
                node = DialogNodes[i];
                return true;
            }
        }

        node = null;
        return false;
    }


    public DialogNode GetDialogNodeById(string nodeId)
    {
        return DialogNodes.Find(node => node.Id == nodeId);
    }
}
