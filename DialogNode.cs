using System.Collections.Generic;
using UnityEngine;

// [System.Serializable]
public class DialogNode: ScriptableObject
{
    [SerializeField] public string Id; // A unique identifier for this node
    [SerializeField] public string DialogText; // The text displayed in this dialog node
    [HideInInspector]
    public List<DialogConnection> inputConnections;
    [HideInInspector]
    public List<DialogConnection> outputConnections;
    public Rect InputPointRect;
    public Rect OutputPointRect;
    public Vector2 Position;
    [SerializeField] public bool IsStartNode;
    [SerializeField] public bool IsEndNode;
    public enum DialogNodeType
    {
        DialogNode,
        OptionNode
    }



    public DialogNode(string id, string dialogText)
    {
        Id = id;
        DialogText = dialogText;
    }
}
