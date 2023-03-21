using UnityEditor;
using UnityEngine;

public class NodeWindow : EditorWindow
{
    private DialogTimelineEditorWindow _dialogTimelineEditorWindow;
    private Vector2 _scrollPos;

    public static void Open(DialogTimelineEditorWindow dialogTimelineEditorWindow)
    {
        NodeWindow window = GetWindow<NodeWindow>("Node Window");
        window._dialogTimelineEditorWindow = dialogTimelineEditorWindow;
    }

    private void OnGUI()
    {
        if (_dialogTimelineEditorWindow == null)
        {
            EditorGUILayout.LabelField("No Dialog Timeline Editor window selected.");
            return;
        }

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        EditorGUILayout.LabelField("Dialog Nodes");
        EditorGUI.indentLevel++;

        for (int i = 0; i < _dialogTimelineEditorWindow.DialogTimelineAsset.DialogNodes.Count; i++)
        {
            EditorGUILayout.LabelField($"{i}: {_dialogTimelineEditorWindow.DialogTimelineAsset.DialogNodes[i].DialogText}");
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Option Nodes");
        EditorGUI.indentLevel++;

        for (int i = 0; i < _dialogTimelineEditorWindow.DialogTimelineAsset.OptionNodes.Count; i++)
        {
            EditorGUILayout.LabelField($"{i}: {_dialogTimelineEditorWindow.DialogTimelineAsset.OptionNodes[i].OptionText}");
        }

        EditorGUI.indentLevel--;

        EditorGUILayout.EndScrollView();
    }
}
