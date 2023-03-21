using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class DialogTimelineEditorWindow : EditorWindow
{

    private List<bool> nodeFoldouts = new List<bool>();
    private List<Rect> nodeWindows = new List<Rect>();
    private List<Rect> optionNodeWindows = new List<Rect>();

    private bool _hasCreatedStartEndNodes = false;

    private Vector2 nodeSize = new Vector2(200, 100);
    private Vector2 _offset = Vector2.zero;
    private Vector2 panOffset = Vector2.zero;
    private Vector2 mousePosOnLastDown;
    private float zoom = 1f;

    private GUIStyle headerStyle;
    private GUIStyle startHeaderStyle;
    private GUIStyle endHeaderStyle;

    private DialogTimelineAsset _dialogTimelineAsset;
    public DialogTimelineAsset DialogTimelineAsset
    {
        get => _dialogTimelineAsset;
        set
        {
            _dialogTimelineAsset = value;
            InitializeNodeWindows();
            _hasCreatedStartEndNodes = false;
        }
    }

    private int _connectionPointBeingDraggedIndex; 
    private ConnectionPoint _connectionPointBeingDragged;

    private bool _isDraggingDialogNode;

    private DialogConnection draggingConnection = null;

    private void DrawNodeInput(DialogNode node)
    {
        if (node.IsEndNode)
        {
            Rect inputRect = new Rect(node.Position.x - 10, node.Position.y + nodeSize.y / 4 - 5, 10, 10);
            EditorGUI.DrawRect(inputRect, Color.black);
            node.InputPointRect = inputRect;
        }
        else
        {
            Rect inputRect = new Rect(node.Position.x - 10, node.Position.y + nodeSize.y / 2 - 5, 10, 10);
            EditorGUI.DrawRect(inputRect, Color.black);
            node.InputPointRect = inputRect;
        }
    }

    private void DrawNodeOutput(DialogNode node)
    {
        if(node.IsStartNode)
        {
            Rect outputRect = new Rect(node.Position.x + nodeSize.x, node.Position.y + nodeSize.y / 4 - 5, 10, 10);
            EditorGUI.DrawRect(outputRect, Color.black);
            node.OutputPointRect = outputRect;
        }
        else
        {
            Rect outputRect = new Rect(node.Position.x + nodeSize.x, node.Position.y + nodeSize.y / 2 - 5, 10, 10);
            EditorGUI.DrawRect(outputRect, Color.black);
            node.OutputPointRect = outputRect;
        }
    }

    private void DrawOptionNodeInput(OptionNode node)
    {
        Rect inputRect = new Rect(node.Position.x - 10, node.Position.y + nodeSize.y / 2 - 5, 10, 10);
        EditorGUI.DrawRect(inputRect, Color.black);
        node.InputPointRect = inputRect;
    }

    private void DrawOptionNodeOutput(OptionNode node)
    {
        Rect outputRect = new Rect(node.Position.x + nodeSize.x, node.Position.y + nodeSize.y / 2 - 5, 10, 10);
        EditorGUI.DrawRect(outputRect, Color.black);
        node.OutputPointRect = outputRect;
    }


    private void HandleContextMenuEvents(Event e)
    {
        if (e.type == EventType.ContextClick)
        {
            for (int i = 0; i < _dialogTimelineAsset.Nodes.Count; i++)
            {
                DialogNode node = _dialogTimelineAsset.Nodes[i];
                for (int j = 0; j < node.Options.Count; j++)
                {
                    DialogOption option = node.Options[j];
                    if (_dialogTimelineAsset.TryGetNodeById(option.TargetNodeId, out DialogNode targetNode))
                    {
                        int targetNodeIndex = _dialogTimelineAsset.Nodes.IndexOf(targetNode);
                        Rect targetNodeWindowRect = nodeWindows[targetNodeIndex];

                        Vector2 start = new Vector2(nodeWindows[i].xMax, nodeWindows[i].center.y);
                        Vector2 end = new Vector2(targetNodeWindowRect.xMin, targetNodeWindowRect.center.y);

                        if (IsMouseOnLine(e.mousePosition, start, end))
                        {
                            ShowContextMenu(option, node);
                            e.Use();
                            return;
                        }
                    }
                }
            }
        }
    }
    private bool IsMouseOnLine(Vector2 mousePos, Vector2 start, Vector2 end)
    {
        const float lineWidth = 4f;
        const float threshold = lineWidth * 4f;

        float distance = Mathf.Abs((end.y - start.y) * mousePos.x - (end.x - start.x) * mousePos.y + end.x * start.y - end.y * start.x) / Mathf.Sqrt(Mathf.Pow(end.y - start.y, 2) + Mathf.Pow(end.x - start.x, 2));

        return distance <= threshold;
    }

    private Texture2D MakeTexture(Color col)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, col);
        tex.Apply();
        return tex;
    }

    public static void ShowEditorWindow(DialogTimelineAsset dialogTimelineAsset)
    {
        DialogTimelineEditorWindow window = GetWindow<DialogTimelineEditorWindow>("Node Editor");
        window.DialogTimelineAsset = dialogTimelineAsset;
        window.Show();
    }


    private Rect GetInputRect(Rect nodeWindowRect)
    {
        DialogNode node = _dialogTimelineAsset.Nodes.FirstOrDefault(n => n.Position == nodeWindowRect.position);
        if (node != null)
        {
            return node.InputPointRect;
        }
        return default(Rect);
    }

    private Rect GetOutputRect(Rect nodeWindowRect)
    {
        DialogNode node = _dialogTimelineAsset.Nodes.FirstOrDefault(n => n.Position == nodeWindowRect.position);
        if (node != null)
        {
            return node.OutputPointRect;
        }
        return default(Rect);
    }


    private void HandleConnectionEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    StartConnectionDrag(e);
                }
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && _connectionPointBeingDragged != null)
                {
                    Repaint();
                    if (e.button == 0 && _connectionPointBeingDragged != null)
                    {
                        draggingConnection.EndPosition = e.mousePosition;
                        Repaint();
                    }
                }
                break;

            case EventType.MouseUp:
                if (e.button == 0 && _connectionPointBeingDragged != null)
                {
                    if (_connectionPointBeingDragged.GetType() == typeof(DialogNode))
                    {
                        HandleDialogNodeConnection(e);
                    }
                    else if (_connectionPointBeingDragged.GetType() == typeof(OptionNode))
                    {
                        HandleOptionNodeConnection(e);
                    }

                    _connectionPointBeingDragged = null;
                    draggingConnection = null;
                }
                break;
        }
    }

    private void StartConnectionDrag(Event e)
    {
        bool connectionDragStarted = false;
        for (int i = 0; i < _dialogTimelineAsset.Nodes.Count; i++)
        {
            if (GetOutputRect(nodeWindows[i]).Contains(e.mousePosition))
            {
                _connectionPointBeingDragged = new ConnectionPoint(_dialogTimelineAsset.Nodes[i].Id, ConnectionPoint.ConnectionPointType.DialogNode);
                connectionDragStarted = true;
                break;
            }
        }

        for (int i = 0; i < _dialogTimelineAsset.OptionNodes.Count; i++)
        {
            if (!connectionDragStarted && GetOutputRect(optionNodeWindows[i]).Contains(e.mousePosition))
            {
                _connectionPointBeingDragged = new ConnectionPoint(_dialogTimelineAsset.OptionNodes[i].Id, ConnectionPoint.ConnectionPointType.OptionNode);
                connectionDragStarted = true;
                break;
            }
        }

        if (_connectionPointBeingDragged != null && connectionDragStarted)
        {
            draggingConnection = new DialogConnection();
            draggingConnection.StartPosition = e.mousePosition;
            e.Use();
        }
    }

    private void HandleDialogNodeConnection(Event e)
    {
        for (int i = 0; i < _dialogTimelineAsset.OptionNodes.Count; i++)
        {
            if (GetInputRect(optionNodeWindows[i]).Contains(e.mousePosition))
            {
                DialogNode dialogNode = _dialogTimelineAsset.Nodes.Find(node => node.Id == _connectionPointBeingDragged.Id);
                string defaultOptionText = "New Option"; // You can change this to any default text you want
                dialogNode.Options.Add(new DialogOption(defaultOptionText, _dialogTimelineAsset.OptionNodes[i].Id, _dialogTimelineAsset.OptionNodes[i].Id));
                break;
            }
        }
    }



    private void HandleOptionNodeConnection(Event e)
    {
        for (int i = 0; i < _dialogTimelineAsset.Nodes.Count; i++)
        {
            if (GetInputRect(nodeWindows[i]).Contains(e.mousePosition))
            {
                OptionNode optionNode = _dialogTimelineAsset.OptionNodes.Find(node => node.Id == _connectionPointBeingDragged.Id);
                optionNode.TargetDialogNodeId = _dialogTimelineAsset.Nodes[i].Id;
                break;
            }
        }
    }

    private void ShowContextMenu(DialogOption option, DialogNode outputNode)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Delete Connection"), false, () => RemoveConnection(option, outputNode));
        menu.ShowAsContext();
    }
    private void RemoveConnection(DialogOption option, DialogNode outputNode)
    {
        outputNode.Options.Remove(option);
    }
    private void DrawOptionNodes()
    {
        for (int i = 0; i < optionNodeWindows.Count; i++)
        {
            optionNodeWindows[i] = GUILayout.Window(i + nodeWindows.Count, optionNodeWindows[i], (id) => DrawOptionNodeWindow(i-1), "", GUILayout.Width(nodeSize.x), GUILayout.Height(nodeSize.y));
            _dialogTimelineAsset.OptionNodes[i].Position = optionNodeWindows[i].position;
            DrawOptionNodeInput(_dialogTimelineAsset.OptionNodes[i]);
            DrawOptionNodeOutput(_dialogTimelineAsset.OptionNodes[i]);
        }
    }

    private void DrawOptionNodeWindow(int id)
    {
        if (id < 0 || id >= _dialogTimelineAsset.OptionNodes.Count)
        {
            Debug.LogError("Invalid option node ID: " + id);
            return;
        }
        OptionNode optionNode = _dialogTimelineAsset.OptionNodes[id];

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Option Node: {optionNode.Id}", headerStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        GUILayout.Label("Option Text:");
        optionNode.OptionText = EditorGUILayout.TextField(optionNode.OptionText);

        GUILayout.Space(10);

        if (GUILayout.Button("Delete"))
        {
            DeleteOptionNode(id);
        }

        GUILayout.Space(10);

        GUI.DragWindow();
    }
    private void DeleteOptionNode(int id)
    {
        if (id >= 0 && id < _dialogTimelineAsset.OptionNodes.Count)
        {
            // Remove any connections related to the option node being deleted
            for (int i = _dialogTimelineAsset.Nodes.Count - 1; i >= 0; i--)
            {
                DialogNode dialogNode = _dialogTimelineAsset.Nodes[i];
                for (int j = dialogNode.Options.Count - 1; j >= 0; j--)
                {
                    if (dialogNode.Options[j].OptionNodeId == _dialogTimelineAsset.OptionNodes[id].Id)
                    {
                        dialogNode.Options.RemoveAt(j);
                    }
                }
            }

            // Remove the option node from the asset and window list
            _dialogTimelineAsset.OptionNodes.RemoveAt(id);
            optionNodeWindows.RemoveAt(id);

            GUI.changed = true;
        }
    }
    public void DrawDialogNodes()
    {
        for (int i = 0; i < nodeWindows.Count; i++)
        {
            if (_dialogTimelineAsset.Nodes[i].IsEndNode)
            {
                nodeWindows[i] = GUILayout.Window(i, nodeWindows[i], DrawNodeWindow, "", GUILayout.Width(nodeSize.x), GUILayout.Height(nodeSize.y / 2));
                DrawNodeInput(_dialogTimelineAsset.Nodes[i]);
            }
            else if (_dialogTimelineAsset.Nodes[i].IsStartNode)
            {
                nodeWindows[i] = GUILayout.Window(i, nodeWindows[i], DrawNodeWindow, "", GUILayout.Width(nodeSize.x), GUILayout.Height(nodeSize.y / 2));
                DrawNodeOutput(_dialogTimelineAsset.Nodes[i]);
            }
            else
            {
                nodeWindows[i] = GUILayout.Window(i, nodeWindows[i], DrawNodeWindow, "", GUILayout.Width(nodeSize.x), GUILayout.Height(nodeSize.y));
                DrawNodeInput(_dialogTimelineAsset.Nodes[i]);
                DrawNodeOutput(_dialogTimelineAsset.Nodes[i]);
            }
        }
    }
    private void OnGUI()
    {
        EditorGUIUtility.AddCursorRect(position, MouseCursor.Pan);

        // Apply pan and zoom transformation
        GUI.BeginGroup(new Rect(position.x + panOffset.x, position.y + panOffset.y, position.width / zoom, position.height / zoom));
        {
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(zoom, zoom, 1));

            if (_dialogTimelineAsset == null)
            {
                EditorGUILayout.LabelField("No DialogTimelineAsset selected.");
                return;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Dialog Node"))
            {
                Object newNode = (DialogNode)CreateNode(new Vector2(10, 30) * nodeWindows.Count);
            }
            else if (GUILayout.Button("Add Option Node"))
            {
                Object newNode = (OptionNode)CreateNode(new Vector2(10, 30) * optionNodeWindows.Count, DialogNode.DialogNodeType.OptionNode);
            }
            EditorGUILayout.EndHorizontal();

            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);

            DrawConnections();

            if (draggingConnection != null)
            {
                Handles.DrawBezier(draggingConnection.StartPosition, draggingConnection.EndPosition, draggingConnection.StartPosition + Vector2.right * 50, draggingConnection.EndPosition - Vector2.right * 50, Color.white, null, 4);
            }

            BeginWindows();
            DrawOptionNodes();
            DrawDialogNodes();
            EndWindows();

            HandleConnectionEvents(Event.current);

            if (GUI.changed) Repaint();
        }
        GUI.EndGroup();

        HandleZoomAndPanEvents(Event.current);
        HandleContextMenuEvents(Event.current);
    }


    private void HandleZoomAndPanEvents(Event e)
    {
        // Handle zooming
        if (e.type == EventType.ScrollWheel)
        {
            // Zoom in or out based on the scroll wheel delta
            zoom += e.delta.y * 0.01f;
            zoom = Mathf.Clamp(zoom, 0.1f, 10f);
            e.Use();
        }

        // Handle panning aa
        if (e.type == EventType.MouseDown && e.button == 2)
        {
            mousePosOnLastDown = e.mousePosition;
            e.Use();
        }
        if (e.type == EventType.MouseDrag && e.button == 2)
        {
            Vector2 delta = (e.mousePosition - mousePosOnLastDown);
            panOffset += delta;
            mousePosOnLastDown = e.mousePosition;
            e.Use();
        }
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        Vector3 newOffset = new Vector3(_offset.x % gridSpacing, _offset.y % gridSpacing, 0);

        // Draw the vertical grid lines
        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i + newOffset.x, 0, 0), new Vector3(gridSpacing * i + newOffset.x, position.height, 0f));
        }

        // Draw the horizontal grid lines
        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(0, gridSpacing * j + newOffset.y, 0), new Vector3(position.width, gridSpacing * j + newOffset.y, 0f));
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }


    private void InitializeNodeWindows()
    {

        if (!_hasCreatedStartEndNodes)
        {
            DialogNode startNode = FindStartOrEndNode(true);
            if (startNode == null)
            {
                startNode = (DialogNode)CreateNode(new Vector2(10, 30) * nodeWindows.Count, isStartNode: true);
            }

            DialogNode endNode = FindStartOrEndNode(false);
            if (endNode == null)
            {
                endNode = (DialogNode)CreateNode(new Vector2(10, 30) * nodeWindows.Count, isEndNode: true);
            }

            _hasCreatedStartEndNodes = true;
        }
        nodeWindows.Clear();
        headerStyle = new GUIStyle(GUI.skin.box);
        headerStyle.normal.textColor = Color.white;

        startHeaderStyle = new GUIStyle(headerStyle);
        startHeaderStyle.normal.background = MakeTexture(Color.green);

        endHeaderStyle = new GUIStyle(headerStyle);
        endHeaderStyle.normal.background = MakeTexture(Color.red);
        for (int i = 0; i < _dialogTimelineAsset.Nodes.Count; i++)
        {
            nodeWindows.Add(new Rect(_dialogTimelineAsset.Nodes[i].Position, nodeSize));
            nodeFoldouts.Add(false);
        }
    }


    private Object CreateNode(Vector2 position, DialogNode.DialogNodeType nodeType = DialogNode.DialogNodeType.DialogNode, bool isStartNode = false, bool isEndNode = false)
    {
        if (nodeType == DialogNode.DialogNodeType.DialogNode)
        {
            DialogNode newNode;
            if (isStartNode)
            {
                newNode = new DialogNode($"Start", "START NODE");
            }
            else if (isEndNode)
            {
                newNode = new DialogNode($"End", "END NODE");
            }
            else
            {
                newNode = new DialogNode($"Node_{_dialogTimelineAsset.Nodes.Count}", "New Dialog Node");
            }
            _dialogTimelineAsset.Nodes.Add(newNode);

            newNode.IsStartNode = isStartNode;
            newNode.IsEndNode = isEndNode;

            nodeWindows.Add(new Rect(position, nodeSize));
            nodeFoldouts.Add(false);

            return newNode;
        }
        else if (nodeType == DialogNode.DialogNodeType.OptionNode)
        {
            Debug.Log("Creating OptionNode...");
            OptionNode newNode = ScriptableObject.CreateInstance<OptionNode>();
            if (newNode == null) Debug.LogError("newNode is null");

            newNode.Position = position;
            _dialogTimelineAsset.OptionNodes.Add(newNode);

            if (_dialogTimelineAsset.OptionNodes == null) Debug.LogError("_dialogTimelineAsset.OptionNodes is null");
            if (optionNodeWindows == null) Debug.LogError("optionNodeWindows is null");

            optionNodeWindows.Add(new Rect(position, nodeSize));
            return newNode;
        }

        else
        {
            Debug.LogError("invalid DialogNodeType Given");
            return null;
        }
    }
    private DialogNode FindStartOrEndNode(bool findStartNode)
    {
        return _dialogTimelineAsset.Nodes.FirstOrDefault(node => findStartNode ? node.IsStartNode : node.IsEndNode);
    }


    private void RemoveNode(int index)
    {
        _dialogTimelineAsset.Nodes.RemoveAt(index);
        nodeWindows.RemoveAt(index);
        nodeFoldouts.RemoveAt(index);
    }
    private void DrawConnections()
    {
        if (_dialogTimelineAsset == null) return;

        for (int i = 0; i < _dialogTimelineAsset.Nodes.Count; i++)
        {
            DialogNode node = _dialogTimelineAsset.Nodes[i];
            Rect nodeWindowRect = nodeWindows[i];

            for (int j = 0; j < node.Options.Count; j++)
            {
                DialogOption option = node.Options[j];

                if (_dialogTimelineAsset.TryGetNodeById(option.TargetNodeId, out DialogNode targetNode))
                {
                    int targetNodeIndex = _dialogTimelineAsset.Nodes.IndexOf(targetNode);
                    Rect targetNodeWindowRect = nodeWindows[targetNodeIndex];

                    Vector2 start = nodeWindowRect.center + new Vector2(nodeWindowRect.width / 2f, 0f);
                    // start = start * zoom + panOffset;

                    Vector2 end = targetNodeWindowRect.center - new Vector2(targetNodeWindowRect.width / 2f, 0f);
                    // end = end * zoom + panOffset;

                    DrawConnection(start, end, Color.white);
                }
            }
        }

        if (draggingConnection != null)
        {
            Vector2 adjustedStartPosition = draggingConnection.StartPosition * zoom + panOffset;
            Vector2 adjustedEndPosition = draggingConnection.EndPosition * zoom + panOffset;
            Handles.DrawBezier(adjustedStartPosition, adjustedEndPosition, adjustedStartPosition + Vector2.right * 50, adjustedEndPosition - Vector2.right * 50, Color.white, null, 4);
        }
    }


    private void DrawConnection(Vector2 start, Vector2 end, Color color)
    {
        GUIStyle connectionStyle = new GUIStyle();
        connectionStyle.normal.background = EditorGUIUtility.whiteTexture;
        Handles.BeginGUI();
        Handles.color = color;
        Handles.DrawBezier(start, end, start + Vector2.right * 50, end - Vector2.right * 50, Color.white, null, 4);
        Handles.EndGUI();
    }


    private void DrawNodeWindow(int id)
    {
        if (id < _dialogTimelineAsset.Nodes.Count)
        {
            // dialog nodes
            // Get the node corresponding to the id
            DialogNode node = _dialogTimelineAsset.Nodes[id];

            GUIStyle nodeBackgroundStyle = new GUIStyle(GUI.skin.box);

            if (node.IsStartNode)
            {
                nodeBackgroundStyle.normal.background = MakeTexture(Color.green);
            }
            else if (node.IsEndNode)
            {
                nodeBackgroundStyle.normal.background = MakeTexture(Color.red);
            }
            else
            {
                nodeBackgroundStyle.normal.background = MakeTexture(Color.gray);
            }

            if (node.IsStartNode)
            {
                GUI.Box(new Rect(0, 0, nodeSize.x, nodeSize.y / 2), "", nodeBackgroundStyle);
                GUILayout.Label("Start");
            }
            else if (node.IsEndNode)
            {
                GUI.Box(new Rect(0, 0, nodeSize.x, nodeSize.y / 2), "", nodeBackgroundStyle);
                GUILayout.Label("End");
            }
            else
            {
                // Update the node's properties
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("ID", GUILayout.Width(30));
                node.Id = EditorGUILayout.TextField(node.Id);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Text", GUILayout.Width(30));
                node.DialogText = EditorGUILayout.TextField(node.DialogText, GUILayout.Height(50));
                EditorGUILayout.EndHorizontal();

                GUIStyle headerStyleToUse = headerStyle;
                if (node.IsStartNode)
                {
                    headerStyleToUse = startHeaderStyle;
                }
                else if (node.IsEndNode)
                {
                    headerStyleToUse = endHeaderStyle;
                } 

                // Remove node button
                if (GUILayout.Button("Remove Node"))
                {
                    _dialogTimelineAsset.Nodes.RemoveAt(id);
                    nodeWindows.RemoveAt(id);
                    return;
                }
            }

            // Make the window draggable
            GUI.DragWindow();

            // Update the node's position
            node.Position = nodeWindows[id].position;
        }
        else
        {
            //option Nodes
            int optionId = id - _dialogTimelineAsset.Nodes.Count;
            OptionNode node = _dialogTimelineAsset.OptionNodes[optionId];

            // Here you can draw the OptionNode's GUI similar to how you did it with DialogNodes
            // For example:
            /*EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Option ID", GUILayout.Width(50));
            node.Id = EditorGUILayout.TextField(node.Id);
            EditorGUILayout.EndHorizontal();*/

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Option Text", GUILayout.Width(70));
            node.OptionText = EditorGUILayout.TextField(node.OptionText, GUILayout.Height(50));
            EditorGUILayout.EndHorizontal();

            // Make the window draggable
            GUI.DragWindow();

            // Update the node's position
            node.Position = optionNodeWindows[optionId].position;
            Debug.Log($"Position of {node.Id} i {node.Position}");
        }
    }
}
