using System;
using UnityEngine;

[Serializable]
public class DialogNode
{
    public string Id = Guid.NewGuid().ToString();
    public string DialogText = "Dialog Text";
    public Rect rect;
    public string TargetOptionNodeId;
    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;

    public DialogNode(Vector2 position, float width, float height, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> onClickInPoint, Action<ConnectionPoint> onClickOutPoint)
    {
        rect = new Rect(position.x, position.y, width, height);
        inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, onClickInPoint);
        outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, onClickOutPoint);
    }

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }
}
