using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogTimelineAsset))]
public class DialogTimelineAssetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.LabelField("Open Node Editor Window", EditorStyles.boldLabel);
        DialogTimelineAsset dialogTimelineAsset = (DialogTimelineAsset)target;
        if (GUILayout.Button("Open Node Editor"))
        {
            DialogTimelineEditorWindow.ShowEditorWindow(dialogTimelineAsset);
        }

    }

}
