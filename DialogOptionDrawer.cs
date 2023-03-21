using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[CustomPropertyDrawer(typeof(DialogOption))]
public class DialogOptionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var optionTextRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        var targetNodeIdRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
        var onOptionSelectedRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 2, position.width, EditorGUIUtility.singleLineHeight);

        EditorGUI.PropertyField(optionTextRect, property.FindPropertyRelative("OptionText"), GUIContent.none);
        EditorGUI.PropertyField(targetNodeIdRect, property.FindPropertyRelative("TargetNodeId"), GUIContent.none);
        EditorGUI.PropertyField(onOptionSelectedRect, property.FindPropertyRelative("OnOptionSelected"), GUIContent.none);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 3;
    }
}
