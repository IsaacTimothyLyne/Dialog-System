using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST : MonoBehaviour
{
    public DialogTimelineAsset dialogTimelineAsset;

    public void OnOptionSelected(DialogOption option)
    {
        switch (option.OnOptionSelected)
        {
            case "Method1":
                Method1();
                break;
            case "Method2":
                Method2();
                break;
            case "Method3":
                Method3();
                break;
            default:
                Debug.LogWarning("No method found with name: " + option.OnOptionSelected);
                break;
        }
    }
    public void Method1()
    {
        Debug.Log("Method 1 called");
    }

    public void Method2()
    {
        Debug.Log("Method 2 called");
    }

    public void Method3()
    {
        Debug.Log("Method 3 called");
    }
}
