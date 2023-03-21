using UnityEngine;
using System;

[System.Serializable]
public class OptionNode : ScriptableObject
{
    public string Id = Guid.NewGuid().ToString();
    public string OptionText = "Option Text";
    public string TargetDialogNodeId;

    public Rect InputPointRect;
    public Rect OutputPointRect;

    public Vector2 Position;
}

