using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetEnabledText : MonoBehaviour
{
    public Text targetText;

    public void SetTextState(bool enabled)
    {
        switch (enabled)
        {
            case true:
                {
                    targetText.color = Color.green;
                    targetText.text = "Enabled";
                    break;
                }
            case false:
                {
                    targetText.color = Color.red;
                    targetText.text = "Disabled";
                    break;
                }
        }
    }
}
