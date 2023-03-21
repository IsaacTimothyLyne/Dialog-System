using System.Collections.Generic;
using UnityEngine;

public class DialogController : MonoBehaviour
{
    [SerializeField] private DialogTimelineAsset dialogTimelineAsset;
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TMPro.TextMeshProUGUI dialogText;
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private GameObject optionButtonPrefab;

    private DialogNode currentNode;

    private void Start()
    {
        if (dialogTimelineAsset == null)
        {
            Debug.LogError("DialogTimelineAsset is not assigned.");
            return;
        }

        StartDialog();
    }

    private void StartDialog()
    {
        currentNode = dialogTimelineAsset.StartNode;
        ShowDialogNode(currentNode);
    }

    private void ShowDialogNode(DialogNode node)
    {
        dialogText.text = node.DialogText;
        dialogPanel.SetActive(true);

        if (node is EndNode)
        {
            optionPanel.SetActive(false);
            return;
        }

        ClearOptions();
        ShowOptions(node);
    }

    private void ShowOptions(DialogNode node)
    {
        optionPanel.SetActive(true);

        // Get the list of OptionNodes connected to the current DialogNode
        List<OptionNode> connectedOptionNodes = dialogTimelineAsset.GetConnectedOptionNodes(node);

        foreach (OptionNode optionNode in connectedOptionNodes)
        {
            GameObject optionButton = Instantiate(optionButtonPrefab, optionPanel.transform);
            TMPro.TextMeshProUGUI buttonText = optionButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            buttonText.text = optionNode.OptionText;

            UnityEngine.UI.Button button = optionButton.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() => OnOptionSelected(optionNode));
        }
    }


    private void OnOptionSelected(OptionNode optionNode)
    {
        DialogNode targetNode = dialogTimelineAsset.DialogNodes.Find(n => n.Id == optionNode.TargetDialogNodeId);

        if (targetNode == null)
        {
            Debug.LogError("Target node not found.");
            return;
        }

        ClearOptions();
        ShowDialogNode(targetNode);
    }

    private void ClearOptions()
    {
        foreach (Transform child in optionPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
