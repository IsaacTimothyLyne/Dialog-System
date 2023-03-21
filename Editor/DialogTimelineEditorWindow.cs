using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DialogTimelineEditorWindow : EditorWindow
{
    private DialogTimelineAsset _dialogTimelineAsset;
    private Vector2 _scrollPosition;
    private GUIStyle _nodeStyle;
    private GUIStyle _selectedNodeStyle;
    private GUIStyle _inPointStyle;
    private GUIStyle _outPointStyle;
    private GUIStyle _optionNodeStyle;

    private float _nodeWidth = 200;
    private float _nodeHeight = 150;
    private float _optionNodeWidth = 100;
    private float _optionNodeHeight = 50;

    private List<NodeWindow> _nodeWindows;
    private List<NodeWindow> _optionNodeWindows;

    [MenuItem("Window/Dialog Timeline Editor")]
    public static void OpenDialogTimelineEditorWindow()
    {
        var window = GetWindow<DialogTimelineEditorWindow>();
        window.titleContent = new GUIContent("Dialog Timeline Editor");
    }

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        _nodeStyle = new GUIStyle();
        _nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        _nodeStyle.border = new RectOffset(12, 12, 12, 12);

        _selectedNodeStyle = new GUIStyle();
        _selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        _selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);

        _inPointStyle = new GUIStyle();
        _inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
        _inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        _inPointStyle.border = new RectOffset(4, 4, 12, 12);

        _outPointStyle = new GUIStyle();
        _outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
        _outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
        _outPointStyle.border = new RectOffset(4, 4, 12, 12);

        _optionNodeStyle = new GUIStyle();
        _optionNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node6.png") as Texture2D;
        _optionNodeStyle.border = new RectOffset(12, 12, 12, 12);

        _nodeWindows = new List<NodeWindow>();
        _optionNodeWindows = new List<NodeWindow>();
    }

    private void OnGUI()
    {
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
        _dialogTimelineAsset = (DialogTimelineAsset)EditorGUILayout.ObjectField(_dialogTimelineAsset, typeof(DialogTimelineAsset), false);
        EditorGUILayout.EndHorizontal();

        if (_dialogTimelineAsset == null)
        {
            EditorGUILayout.HelpBox("Please select a Dialog Timeline Asset.", MessageType.Warning);
        }
        else
        {
            SelectDialogTimelineAsset();
        }
    }
    private void SelectDialogTimelineAsset()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        BeginWindows();

        // Draw Dialog Nodes
        if (_dialogTimelineAsset.DialogNodes != null)
        {
            _nodeWindows.Clear();
            for (int i = 0; i < _dialogTimelineAsset.DialogNodes.Count; i++)
            {
                DialogNode dialogNode = _dialogTimelineAsset.DialogNodes[i];
                _nodeWindows.Add(new NodeWindow(new Rect(dialogNode.Position, new Vector2(_nodeWidth, _nodeHeight)), _nodeStyle, _selectedNodeStyle, _inPointStyle, _outPointStyle, dialogNode));
            }

            for (int i = 0; i < _nodeWindows.Count; i++)
            {
                _nodeWindows[i].Draw();
            }
        }

        // Draw Option Nodes
        if (_dialogTimelineAsset.OptionNodes != null)
        {
            _optionNodeWindows.Clear();
            for (int i = 0; i < _dialogTimelineAsset.OptionNodes.Count; i++)
            {
                OptionNode optionNode = _dialogTimelineAsset.OptionNodes[i];
                _optionNodeWindows.Add(new NodeWindow(new Rect(optionNode.Position, new Vector2(_optionNodeWidth, _optionNodeHeight)), _optionNodeStyle, _selectedNodeStyle, _inPointStyle, _outPointStyle, optionNode));
            }

            for (int i = 0; i < _optionNodeWindows.Count; i++)
            {
                _optionNodeWindows[i].Draw();
            }
        }

        DrawConnections();
        DrawConnectionHelpers();

        EndWindows();

        EditorGUILayout.EndScrollView();

        ProcessEvents();
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, 0, 0), new Vector3(gridSpacing * i, position.height, 0));
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(0, gridSpacing * j, 0), new Vector3(position.width, gridSpacing * j, 0));
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }
    private void DrawConnections()
    {
        if (_dialogTimelineAsset.DialogConnections != null)
        {
            for (int i = 0; i < _dialogTimelineAsset.DialogConnections.Count; i++)
            {
                DialogConnection dialogConnection = _dialogTimelineAsset.DialogConnections[i];
                NodeWindow startNode = _nodeWindows.FirstOrDefault(n => n.DialogNode.Id == dialogConnection.StartNodeId);
                NodeWindow endNode = _nodeWindows.FirstOrDefault(n => n.DialogNode.Id == dialogConnection.EndNodeId);

                if (startNode != null && endNode != null)
                {
                    DrawConnectionLine(startNode.OutPoint.rect.center, endNode.InPoint.rect.center);
                }
            }
        }
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

    private void ProcessEvents()
    {
        Event e = Event.current;

        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    ClearConnectionSelection();
                }

                if (e.button == 1)
                {
                    ProcessContextMenu(e.mousePosition);
                }
                break;
        }
    }

    private void ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add Dialog Node"), false, () => OnClickAddDialogNode(mousePosition));
        genericMenu.AddItem(new GUIContent("Add Option Node"), false, () => OnClickAddOptionNode(mousePosition));
        genericMenu.ShowAsContext();
    }

    private void OnClickAddDialogNode(Vector2 mousePosition)
    {
        DialogNode newNode = ScriptableObject.CreateInstance<DialogNode>();
        newNode.Position = mousePosition;

        Undo.RecordObject(_dialogTimelineAsset, "Add Dialog Node");
        _dialogTimelineAsset.AddDialogNode(newNode);
        EditorUtility.SetDirty(_dialogTimelineAsset);
    }

    private void OnClickAddOptionNode(Vector2 mousePosition)
    {
        OptionNode newOptionNode = ScriptableObject.CreateInstance<OptionNode>();
        newOptionNode.Position = mousePosition;

        Undo.RecordObject(_dialogTimelineAsset, "Add Option Node");
        _dialogTimelineAsset.AddOptionNode(newOptionNode);
        EditorUtility.SetDirty(_dialogTimelineAsset);
    }

    private void DrawConnectionLine(Vector2 start, Vector2 end)
    {
        Handles.DrawBezier(
            start,
            end,
            start + Vector2.left * 50f,
            end - Vector2.left * 50f,
            Color.white,
            null,
            2f
        );
    }
}

