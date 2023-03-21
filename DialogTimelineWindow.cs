using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DialogTimelineWindow : EditorWindow
{
    private DialogTimelineAsset _dialogTimelineAsset;
    private Vector2 _scrollPosition;


    [MenuItem("Window/Dialog Timeline")]
    public static void ShowWindow()
    {
        GetWindow<DialogTimelineWindow>("Dialog Timeline");
    }
    private void DrawArrow(Vector2 start, Vector2 end, Color color)
    {
        // Draw a simple arrow between two points
        Handles.color = color;
        Handles.DrawLine(new Vector3(start.x, start.y, 0), new Vector3(end.x, end.y, 0));
        Vector2 direction = (start - end).normalized;
        Vector2 arrowHeadPosition = end + direction * 10f;
        Handles.DrawLine(new Vector3(end.x, end.y, 0), new Vector3(arrowHeadPosition.x, arrowHeadPosition.y, 0) + (Vector3)(Quaternion.Euler(0, 0, 30) * direction * 0.6f));
        Handles.DrawLine(new Vector3(end.x, end.y, 0), new Vector3(arrowHeadPosition.x, arrowHeadPosition.y, 0) + (Vector3)(Quaternion.Euler(0, 0, -30) * direction * 0.6f));
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Dialog Timeline Asset:", GUILayout.Width(150));
        _dialogTimelineAsset = (DialogTimelineAsset)EditorGUILayout.ObjectField(_dialogTimelineAsset, typeof(DialogTimelineAsset), false);
        EditorGUILayout.EndHorizontal();

        if (_dialogTimelineAsset == null)
        {
            EditorGUILayout.HelpBox("Select a Dialog Timeline Asset to edit.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dialog Nodes:", EditorStyles.boldLabel);

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        // Loop through each DialogNode in the DialogTimelineAsset
        for (int i = 0; i < _dialogTimelineAsset.Nodes.Count; i++)
        {
            DialogNode currentNode = _dialogTimelineAsset.Nodes[i];

            EditorGUILayout.BeginVertical(GUI.skin.box);
            currentNode.DialogText = EditorGUILayout.TextField("Dialog Text:", currentNode.DialogText);

            // Display the options connected to this node
            // (You'll need to expand this to display the DialogOption instances)

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();

        // Controls for creating new nodes and options
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Create New Node"))
        {
            // Generate a unique ID for the new node
            string newNodeId = Guid.NewGuid().ToString();
            _dialogTimelineAsset.AddDialogNode(new DialogNode(newNodeId, "New Dialog Node"));
        }

        EditorGUILayout.EndHorizontal();

        // Loop through each DialogNode in the DialogTimelineAsset (again)
        for (int i = 0; i < _dialogTimelineAsset.Nodes.Count; i++)
        {
            DialogNode currentNode = _dialogTimelineAsset.Nodes[i];

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Node ID: " + currentNode.Id);

            for (int j = 0; j < currentNode.Options.Count; j++)
            {
                DialogOption currentOption = currentNode.Options[j];

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Option " + (j + 1) + ":", GUILayout.Width(60));
                currentOption.OptionText = EditorGUILayout.TextField(currentOption.OptionText, GUILayout.Width(150));
                currentOption.TargetNodeId = EditorGUILayout.TextField("Target Node ID:", currentOption.TargetNodeId);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Connections:", EditorStyles.boldLabel);

        // Loop through each DialogNode in the DialogTimelineAsset to visualize connections
        for (int i = 0; i < _dialogTimelineAsset.Nodes.Count; i++)
        {
            DialogNode currentNode = _dialogTimelineAsset.Nodes[i];

            for (int j = 0; j < currentNode.Options.Count; j++)
            {
                DialogOption currentOption = currentNode.Options[j];
                DialogNode targetNode = _dialogTimelineAsset.GetNodeById(currentOption.TargetNodeId);

                if (targetNode != null)
                {
                    // Calculate the start and end positions for the arrow (adjust these as needed)
                    Vector2 startPos = new Vector2(100 + 200 * i, 50 + 100 * j);
                    Vector2 endPos = new Vector2(100 + 200 * (i + 1), 50 + 100 * (_dialogTimelineAsset.Nodes.IndexOf(targetNode)));

                    DrawArrow(startPos, endPos, Color.green);
                }
            }
        }
        // Loop through each DialogNode in the DialogTimelineAsset to handle node deletion and connection editing
        for (int i = 0; i < _dialogTimelineAsset.Nodes.Count; i++)
        {
            DialogNode currentNode = _dialogTimelineAsset.Nodes[i];

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Node ID: " + currentNode.Id);

            for (int j = 0; j < currentNode.Options.Count; j++)
            {
                DialogOption currentOption = currentNode.Options[j];

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Option " + (j + 1) + ":", GUILayout.Width(60));
                currentOption.OptionText = EditorGUILayout.TextField(currentOption.OptionText, GUILayout.Width(150));
                currentOption.TargetNodeId = EditorGUILayout.TextField("Target Node ID:", currentOption.TargetNodeId);

                if (GUILayout.Button("Delete Option", GUILayout.Width(100)))
                {
                    currentNode.Options.RemoveAt(j);
                    j--; // Adjust the loop index to account for the removed option
                }

                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Delete Node"))
            {
                _dialogTimelineAsset.RemoveDialogNode(i);
                i--; // Adjust the loop index to account for the removed node
                break; // Break the loop to avoid possible index-related issues
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }
}
