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

/* to say to chat GPT:

DialogConnections isnt present in the DialogTimelineAsset script as you can see in the github here:
https://github.com/IsaacTimothyLyne/Dialog-System

How should i impliment this and what other scripts should i change to update this

 */
