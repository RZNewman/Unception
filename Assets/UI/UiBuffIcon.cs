using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiBuffIcon : MonoBehaviour
{
    public TMP_Text display;

    public void setDisplay(string text, Color c)
    {
        display.text = text;
        display.color = c;
    }
}
