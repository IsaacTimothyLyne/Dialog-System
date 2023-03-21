using UnityEngine;
using System;

[System.Serializable]
public class StartEndNode: ScriptableObject
{
    public string Id = Guid.NewGuid().ToString();
    public string TargetNodeId;
    [SerializeField] public bool IsStartNode;
    [SerializeField] public bool IsEndNode;

    public Rect InputPointRect;
    public Rect OutputPointRect;

    public Vector2 Position;

    public StartEndNode(bool isStartNode, bool isEndNode, string id, Vector2 position) {
        Id = id;
        IsEndNode= isEndNode;
        IsStartNode= isStartNode;
        Position= position;
    }
}