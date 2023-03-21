using UnityEngine;

public enum ConnectionPointType { In, Out }

public class ConnectionPoint
{
    public Rect rect;
    public ConnectionPointType type;
    public DialogNode dialogNode;
    public OptionNode optionNode;
    public GUIStyle style;
    public System.Action<ConnectionPoint> OnClickConnectionPoint;

    public ConnectionPoint(DialogNode dialogNode, ConnectionPointType type, GUIStyle style, System.Action<ConnectionPoint> OnClickConnectionPoint)
    {
        this.dialogNode = dialogNode;
        this.type = type;
        this.style = style;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        rect = new Rect(0, 0, 10f, 20f);
    }

    public ConnectionPoint(OptionNode optionNode, ConnectionPointType type, GUIStyle style, System.Action<ConnectionPoint> OnClickConnectionPoint)
    {
        this.optionNode = optionNode;
        this.type = type;
        this.style = style;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        rect = new Rect(0, 0, 10f, 20f);
    }

    public void Draw()
    {
        rect.y = (type == ConnectionPointType.In) ? dialogNode.rect.y + (dialogNode.rect.height * 0.5f) : optionNode.rect.y + (optionNode.rect.height * 0.5f);
        rect.x = (type == ConnectionPointType.In) ? dialogNode.rect.x - rect.width + 8f : optionNode.rect.x + optionNode.rect.width - 8f;

        if (GUI.Button(rect, "", style))
        {
            if (OnClickConnectionPoint != null)
            {
                OnClickConnectionPoint(this);
            }
        }
    }
}
