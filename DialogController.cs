using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Reflection;
using TMPro;


public class DialogController : MonoBehaviour
{
    [SerializeField] private DialogTimelineAsset dialogTimelineAsset;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Button optionButtonPrefab;
    [SerializeField] private Transform optionsContainer;

    [System.Serializable] public class DialogEndEvent : UnityEvent { }
    public DialogEndEvent OnDialogEnd;

    private DialogNode currentNode;

    private void Start()
    {
        InitializeDialogSystem();
    }

    public void StartDialog(DialogTimelineAsset dialogAsset)
    {
        dialogTimelineAsset = dialogAsset;
        InitializeDialogSystem();
    }

    private void InitializeDialogSystem()
    {
        if (dialogTimelineAsset == null)
        {
            Debug.LogError("DialogTimelineAsset is not assigned. Please call StartDialog() with a valid DialogTimelineAsset.");
            return;
        }

        // Find the UI elements in the scene if they are not assigned in the inspector
        if (dialogText == null)
        {
            dialogText = FindObjectOfType<TextMeshProUGUI>();
        }

        if (optionsContainer == null)
        {
            optionsContainer = FindObjectOfType<Canvas>().transform;
        }

        // Remove any existing layout group components
        HorizontalLayoutGroup horizontalLayoutGroup = optionsContainer.GetComponent<HorizontalLayoutGroup>();
        if (horizontalLayoutGroup != null)
        {
            Destroy(horizontalLayoutGroup);
        }

        VerticalLayoutGroup verticalLayoutGroup = optionsContainer.GetComponent<VerticalLayoutGroup>();
        if (verticalLayoutGroup != null)
        {
            Destroy(verticalLayoutGroup);
        }

        // Add the layout group based on the chosen layout direction
        if (dialogTimelineAsset.OptionsLayoutDirection == DialogTimelineAsset.LayoutDirection.Horizontal)
        {
            optionsContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        }
        else
        {
            optionsContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        // Create the option button instances and hide them initially
        for (int i = 0; i < dialogTimelineAsset.MaxOptionsCount; i++)
        {
            Button optionButton = Instantiate(optionButtonPrefab, optionsContainer);
            optionButton.gameObject.SetActive(false);
        }

        // Assign the initial DialogNode (you can use the first node in the list or use a specific starting node)
        currentNode = dialogTimelineAsset.Nodes[1];

        // Update the dialog display
        UpdateDialogDisplay();
    }


    private void UpdateDialogDisplay()
    {
        if (currentNode == null || dialogTimelineAsset == null)
        {
            Debug.LogError("Current node or DialogTimelineAsset is not assigned. Make sure InitializeDialogSystem() has been called.");
            return;
        }

        if(currentNode.IsEndNode)
        {
            EndDialog();
            return;
        }

        // Display the current node's dialog text
        dialogText.text = currentNode.DialogText;

        // Clear previous options
        foreach (Transform child in optionsContainer)
        {
            child.gameObject.SetActive(false);
        }

        // Display the current node's options
        int optionIndex = 0;
        foreach (DialogOption option in currentNode.Options)
        {
            if (optionIndex < optionsContainer.childCount && CheckOptionConditions(option))
            {
                // Get the option button from the existing pool of buttons
                Button optionButton = optionsContainer.GetChild(optionIndex).GetComponent<Button>();
                optionButton.gameObject.SetActive(true);

                // Set the option text and remove all previously assigned listeners
                optionButton.GetComponentInChildren<TextMeshProUGUI>().text = option.OptionText;
                optionButton.onClick.RemoveAllListeners();

                // Assign the new listener for this option
                optionButton.onClick.AddListener(() => OnOptionClicked(option));

                optionIndex++;
            }
            else
            {
                Debug.LogWarning("Not enough option buttons in the optionsContainer. Consider increasing the MaxOptionsCount in the DialogTimelineAsset.");
                break;
            }
        }

        // Hide any unused option buttons
        for (int i = optionIndex; i < optionsContainer.childCount; i++)
        {
            optionsContainer.GetChild(i).gameObject.SetActive(false);
        }
    }

    private bool CheckOptionConditions(DialogOption option) // TODO: Complete this
    {
        // Return true if no condition is set
        if (option.Condition.Type == DialogCondition.ConditionType.None)
        {
            return true;
        }

        // Find the component containing the variable
        Component component = GameObject.FindWithTag("Player").GetComponent(option.Condition.ComponentName);
        if (component == null)
        {
            Debug.LogWarning($"Component '{option.Condition.ComponentName}' not found.");
            return false;
        }

        // Get the variable's value using reflection
        System.Reflection.FieldInfo field = component.GetType().GetField(option.Condition.VariableName);
        if (field == null)
        {
            Debug.LogWarning($"Variable '{option.Condition.VariableName}' not found in component '{option.Condition.ComponentName}'.");
            return false;
        }

        // Evaluate the condition based on its type
        if (option.Condition.Type == DialogCondition.ConditionType.Boolean)
        {
            bool variableValue = (bool)field.GetValue(component);
            return variableValue == option.Condition.BooleanValue;
        }
        else if (option.Condition.Type == DialogCondition.ConditionType.Float)
        {
            float variableValue = (float)field.GetValue(component);

            if (option.Condition.FloatComparison == DialogCondition.ComparisonOperator.GreaterThanOrEqual)
            {
                return variableValue >= option.Condition.FloatValue;
            }
            else // ComparisonOperator.LessThanOrEqual
            {
                return variableValue <= option.Condition.FloatValue;
            }
        }

        // Default to false if the condition type is not recognized
        return false;
    }

    private void OnOptionClicked(DialogOption option)
    {
        // Change the current node to the target node specified in the DialogOption
        currentNode = dialogTimelineAsset.GetNodeById(option.TargetNodeId);

        if (currentNode != null)
        {
            // Update the dialog display
            UpdateDialogDisplay();
        }
        else
        {
            // End the dialog
            EndDialog();
        }
        // Call the method specified by the OnOptionSelected string
        if (!string.IsNullOrEmpty(option.OnOptionSelected))
        {
            MethodInfo method = GetType().GetMethod(option.OnOptionSelected, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(this, null);
            }
            else
            {
                Debug.LogError("Method not found: " + option.OnOptionSelected);
            }
        }
    }

    private void EndDialog()
    {
        // Hide the dialog UI elements (e.g., dialog text and option buttons)
        dialogText.gameObject.SetActive(false);
        foreach (Transform child in optionsContainer)
        {
            child.gameObject.SetActive(false);
        }

        // Trigger the OnDialogEnd event if assigned
        OnDialogEnd?.Invoke();
    }

    // ... (Optional: Implement a system for executing actions based on the dialog choices)
}
