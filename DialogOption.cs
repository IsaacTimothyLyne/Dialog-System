using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DialogOption
{
    public string OptionText; // The text displayed for this option
    public string TargetNodeId; // The ID of the target DialogNode this option leads to
    public DialogCondition Condition;
    public string OnOptionSelected = "";
    public List<DialogOption> Options;
    public string OptionNodeId;



    public DialogOption(string optionText, string targetNodeId, string optionNodeId)
    {
        OptionText = optionText;
        TargetNodeId = targetNodeId;
        OptionNodeId = optionNodeId;
    }

}

[System.Serializable]
public class DialogCondition
{
    public enum ConditionType
    {
        None,
        Boolean,
        Float
    }

    public ConditionType Type;
    public string ComponentName;
    public string VariableName;
    public bool BooleanValue;
    public float FloatValue;
    public ComparisonOperator FloatComparison;

    public enum ComparisonOperator
    {
        GreaterThanOrEqual,
        LessThanOrEqual
    }
}