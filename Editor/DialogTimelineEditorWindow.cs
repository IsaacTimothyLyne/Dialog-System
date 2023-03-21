using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DialogTimelineEditorWindow : EditorWindow
{
    private DialogTimelineAsset _dialogTimelineAsset;
    private Vector2 _scrollPosition;
    private GUIStyle headerStyle;
    private Vector2 nodeSize = new Vector2(300, 100);

    private List<Rect> nodeWindows = new List<Rect>();
    private List<Rect> optionNodeWindows = new List<Rect>();

    private ConnectionPoint selectedInPoint;
    private ConnectionPoint selectedOutPoint;


    [MenuItem("Window/Dialog System/Dialog Timeline Editor")]
    public static void ShowWindow()
    {
        GetWindow<DialogTimelineEditorWindow>("Dialog Timeline Editor");
    }

    private void OnEnable()
    {
        headerStyle = new GUIStyle();
        headerStyle.fontSize = 16;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.MiddleCenter;
    }

    private void OnGUI()
    {
        if (_dialogTimelineAsset == null)
        {
            EditorGUILayout.HelpBox("Please select a Dialog Timeline Asset to edit.", MessageType.Warning);
            return;
        }

        DrawToolbar();
        DrawNodes();
        DrawOptionNodes();
        DrawConnections();
        DrawConnectionHelpers();
        ProcessEvents();
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Select Dialog Timeline Asset", EditorStyles.toolbarButton))
        {
            SelectDialogTimelineAsset();
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
    private void DrawConnections()
    {
        if (_dialogTimelineAsset == null) return;

        for (int i = 0; i < _dialogTimelineAsset.Nodes.Count; i++)
        {
            DialogNode node = _dialogTimelineAsset.Nodes[i];
            for (int j = 0; j < node.Options.Count; j++)
            {
                Option option = node.Options[j];
                OptionNode optionNode = _dialogTimelineAsset.GetOptionNodeById(option.OptionNodeId);
                if (optionNode != null)
                {
                    DrawNodeConnection(node.OutputPointRect.center, optionNode.InputPointRect.center);
                }
            }
        }
    }
    private void DrawNodeConnection(Vector2 start, Vector2 end)
    {
        Vector3 startPosition = new Vector3(start.x, start.y, 0);
        Vector3 endPosition = new Vector3(end.x, end.y, 0);
        Vector3 controlPointOffset = end - start;
        controlPointOffset.y = 0;
        controlPointOffset.x *= 0.8f;
        Vector3 startControlPoint = startPosition + controlPointOffset;
        Vector3 endControlPoint = endPosition - controlPointOffset;

        Color connectionColor = Color.green;
        Handles.DrawBezier(startPosition, endPosition, startControlPoint, endControlPoint, connectionColor, null, 3f);
    }
    private void DrawConnectionHelpers()
    {
        if (selectedInPoint != null && selectedOutPoint == null)
        {
            Handles.DrawBezier(
                selectedInPoint.rect.center,
                Event.current.mousePosition,
                selectedInPoint.rect.center + Vector2.left * 50f,
                Event.current.mousePosition - Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }

        if (selectedOutPoint != null && selectedInPoint == null)
        {
            Handles.DrawBezier(
                selectedOutPoint.rect.center,
                Event.current.mousePosition,
                selectedOutPoint.rect.center - Vector2.left * 50f,
                Event.current.mousePosition + Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }
    }

    private void OnClickInPoint(ConnectionPoint inPoint)
    {
        selectedInPoint = inPoint;
    }
    private void OnClickOutPoint(ConnectionPoint outPoint)
    {
        selectedOutPoint = outPoint;
    }
    private void OnClickRemovePoint()
    {
        if (selectedInPoint != null)
        {
            selectedInPoint = null;
        }

        if (selectedOutPoint != null)
        {
            selectedOutPoint = null;
        }
    }

    private void ProcessEvents()
    {
        // Your code for processing events in the editor window
    }

    private void SelectDialogTimelineAsset()
    {
        EditorGUI.BeginChangeCheck();
        _dialogTimelineAsset = EditorGUILayout.ObjectField("Dialog Timeline Asset", _dialogTimelineAsset, typeof(DialogTimelineAsset), false) as DialogTimelineAsset;
        if (EditorGUI.EndChangeCheck() && _dialogTimelineAsset != null)
        {
            Initialize();
        }
    }
    private void Initialize()
    {
        nodeSize = new Vector2(200, 150);
        _dialogTimelineAsset = null;
        nodeWindows = new List<Rect>();
        optionNodeWindows = new List<Rect>();

        GUIStyle headerStyle = new GUIStyle();
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = Color.white;
        headerStyle.fontSize = 16;

        if (_dialogTimelineAsset != null)
        {
            for (int i = 0; i < _dialogTimelineAsset.Nodes.Count; i++)
            {
                DialogNode dialogNode = _dialogTimelineAsset.Nodes[i];
                Rect newNodeWindow = new Rect(dialogNode.Position.x, dialogNode.Position.y, nodeSize.x, nodeSize.y);
                nodeWindows.Add(newNodeWindow);
            }

            for (int i = 0; i < _dialogTimelineAsset.OptionNodes.Count; i++)
            {
                OptionNode optionNode = _dialogTimelineAsset.OptionNodes[i];
                Rect newOptionNodeWindow = new Rect(optionNode.Position.x, optionNode.Position.y, nodeSize.x, nodeSize.y);
                optionNodeWindows.Add(newOptionNodeWindow);
            }
        }
    }
    private void DrawNodes()
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

    private void DrawNodeWindow(int id)
    {
        DialogNode node = _dialogTimelineAsset.Nodes[id];

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Dialog Node: {node.Id}", headerStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        GUILayout.Label("Dialog Text:");
        node.DialogText = EditorGUILayout.TextArea(node.DialogText, GUILayout.Height(50));

        GUILayout.Space(10);

        if (!node.IsStartNode && !node.IsEndNode && GUILayout.Button("Delete"))
        {
            DeleteNode(id);
        }

        GUILayout.Space(10);

        GUI.DragWindow();
    }

    private void DeleteNode(int id)
    {
        if (id >= 0 && id < _dialogTimelineAsset.Nodes.Count)
        {
            // Remove any connections related to the node being deleted
            for (int i = _dialogTimelineAsset.Nodes.Count - 1; i >= 0; i--)
            {
                DialogNode dialogNode = _dialogTimelineAsset.Nodes[i];
                for (int j = dialogNode.Options.Count - 1; j >= 0; j--)
                {
                    if (dialogNode.Options[j].TargetDialogNodeId == _dialogTimelineAsset.Nodes[id].Id)
                    {
                        dialogNode.Options.RemoveAt(j);
                    }
                }
            }

            // Remove the node from the asset and window list
            _dialogTimelineAsset.Nodes.RemoveAt(id);
            nodeWindows.RemoveAt(id);

            GUI.changed = true;
        }
    }

    private void DrawNodeInput(DialogNode node)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("In");
        GUILayout.Space(10);
        Rect inputRect = GUILayoutUtility.GetLastRect();
        inputRect.x += 10;
        inputRect.width = 10;
        inputRect.height = 10;
        EditorGUI.DrawRect(inputRect, Color.black);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        node.InputPointRect = inputRect;
    }

    private void DrawNodeOutput(DialogNode node)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Out");
        GUILayout.Space(10);
        Rect outputRect = GUILayoutUtility.GetLastRect();
        outputRect.x += 10;
        outputRect.width = 10;
        outputRect.height = 10;
        EditorGUI.DrawRect(outputRect, Color.black);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        node.OutputPointRect = outputRect;
    }

    private void DrawOptionNodes()
    {
        for (int i = 0; i < optionNodeWindows.Count; i++)
        {
            optionNodeWindows[i] = GUILayout.Window(i + nodeWindows.Count, optionNodeWindows[i], DrawOptionNodeWindow, "", GUILayout.Width(nodeSize.x), GUILayout.Height(nodeSize.y));
            DrawOptionNodeInput(_dialogTimelineAsset.OptionNodes[i]);
            DrawOptionNodeOutput(_dialogTimelineAsset.OptionNodes[i]);
        }
    }

    private void DrawOptionNodeWindow(int id)
    {
        OptionNode node = _dialogTimelineAsset.OptionNodes[id];

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Option Node: {node.Id}", headerStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        GUILayout.Label("Option Text:");
        node.OptionText = EditorGUILayout.TextArea(node.OptionText, GUILayout.Height(50));

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
            // Remove the option node from the asset and window list
            _dialogTimelineAsset.OptionNodes.RemoveAt(id);
            optionNodeWindows.RemoveAt(id);

            GUI.changed = true;
        }
    }

    private void DrawOptionNodeInput(OptionNode node)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("In");
        GUILayout.Space(10);
        Rect inputRect = GUILayoutUtility.GetLastRect();
        inputRect.x += 10;
        inputRect.width = 10;
        inputRect.height = 10;
        EditorGUI.DrawRect(inputRect, Color.black);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        node.InputPointRect = inputRect;
    }

    private void DrawOptionNodeOutput(OptionNode node)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Out");
        GUILayout.Space(10);
        Rect outputRect = GUILayoutUtility.GetLastRect();
        outputRect.x += 10;
        outputRect.width = 10;
        outputRect.height = 10;
        EditorGUI.DrawRect(outputRect, Color.black);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        node.OutputPointRect = outputRect;
    }
}