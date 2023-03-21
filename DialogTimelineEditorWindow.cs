using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DialogTimelineEditorWindow : EditorWindow
{
    private DialogTimelineAsset _dialogTimelineAsset;
    private GUIStyle _nodeStyle;
    private GUIStyle _selectedNodeStyle;
    private GUIStyle _inPointStyle;
    private GUIStyle _outPointStyle;
    private GUIStyle _optionNodeStyle;

    private ConnectionPoint selectedInPoint;
    private ConnectionPoint selectedOutPoint;

    private Vector2 _offset;
    private Vector2 _drag;

    [MenuItem("Dialog System/Dialog Timeline Editor")]
    public static void Open()
    {
        DialogTimelineEditorWindow window = GetWindow<DialogTimelineEditorWindow>();
        window.titleContent = new GUIContent("Dialog Timeline Editor");
    }

    private void OnEnable()
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
        _optionNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node3.png") as Texture2D;
        _optionNodeStyle.border = new RectOffset(12, 12, 12, 12);
    }

    private void OnGUI()
    {
        if (_dialogTimelineAsset == null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Dialog Timeline Asset:");
            _dialogTimelineAsset = EditorGUILayout.ObjectField(_dialogTimelineAsset, typeof(DialogTimelineAsset), false) as DialogTimelineAsset;
            EditorGUILayout.EndHorizontal();

            if (_dialogTimelineAsset != null)
            {
                Initialize();
            }

            return;
        }
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        DrawNodes();
        DrawConnections();
        DrawConnectionHelpers();

        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        if (GUI.changed)
        {
            Repaint();
        }
    }
    private void Initialize()
    {
        if (Selection.activeObject == null) return;
        if (Selection.activeObject.GetType() != typeof(DialogTimelineAsset)) return;

        _dialogTimelineAsset = (DialogTimelineAsset)Selection.activeObject;
        InitializeConnectionPoints();
    }

    private void InitializeConnectionPoints()
    {
        if (_dialogTimelineAsset.DialogNodes != null)
        {
            for (int i = 0; i < _dialogTimelineAsset.DialogNodes.Count; i++)
            {
                _dialogTimelineAsset.DialogNodes[i].InitializeConnectionPoints(OnClickInPoint, OnClickOutPoint);
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

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        _offset += _drag * 0.5f;
        Vector3 newOffset = new Vector3(_offset.x % gridSpacing, _offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }
    private void DrawNodes()
    {
        if (_dialogTimelineAsset.DialogNodes != null)
        {
            for (int i = 0; i < _dialogTimelineAsset.DialogNodes.Count; i++)
            {
                DialogNode node = _dialogTimelineAsset.DialogNodes[i];
                node.Draw();
            }
        }
    }
    private void DrawConnections()
    {
        if (_dialogTimelineAsset.DialogConnections != null)
        {
            for (int i = 0; i < _dialogTimelineAsset.DialogConnections.Count; i++)
            {
                DialogConnection connection = _dialogTimelineAsset.DialogConnections[i];
                connection.Draw();
            }
        }
    }
    private void ProcessEvents(Event e)
    {
        _drag = Vector2.zero;

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

            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    OnDrag(e.delta);
                }
                break;
        }
    }
    private void ProcessNodeEvents(Event e)
    {
        if (_dialogTimelineAsset.DialogNodes != null)
        {
            for (int i = _dialogTimelineAsset.DialogNodes.Count - 1; i >= 0; i--)
            {
                bool guiChanged = _dialogTimelineAsset.DialogNodes[i].ProcessEvents(e);

                if (guiChanged)
                {
                    GUI.changed = true;
                }
            }
        }
    }
    private void ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add dialog node"), false, () => OnClickAddDialogNode(mousePosition));
        genericMenu.ShowAsContext();
    }
    private void OnClickAddDialogNode(Vector2 mousePosition)
    {
        if (_dialogTimelineAsset != null)
        {
            _dialogTimelineAsset.AddDialogNode(mousePosition);
        }
    }
    private void OnDrag(Vector2 delta)
    {
        _drag = delta;

        if (_dialogTimelineAsset.DialogNodes != null)
        {
            for (int i = 0; i < _dialogTimelineAsset.DialogNodes.Count; i++)
            {
                _dialogTimelineAsset.DialogNodes[i].Drag(delta);
            }
        }

        GUI.changed = true;
    }

    private void ClearConnectionSelection()
    {
        selectedInPoint = null;
        selectedOutPoint = null;
    }
    private void OnClickOutPoint(ConnectionPoint outPoint)
    {
        selectedOutPoint = outPoint;

        if (selectedInPoint != null)
        {
            if (selectedOutPoint.Node != selectedInPoint.Node)
            {
                CreateConnection();
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }
    private void CreateConnection()
    {
        if (_dialogTimelineAsset != null)
        {
            _dialogTimelineAsset.AddConnection(selectedInPoint, selectedOutPoint);
        }
    }
}