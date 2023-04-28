using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogTimelineAsset", menuName = "Dialog System/Dialog Timeline Asset", order = 0)]
public class DialogTimelineAsset : ScriptableObject
{
    public List<DialogNode> DialogNodes;
    public List<OptionNode> OptionNodes;
    [SerializeField]
    public List<DialogConnection> DialogConnections = new List<DialogConnection>();


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
    public void AddDialogNode(DialogNode newNode)
    {
        DialogNodes.Add(newNode);
    }

    public void AddOptionNode(OptionNode newOptionNode)
    {
        OptionNodes.Add(newOptionNode);
    }

}
