using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiStatLabel : MonoBehaviour
{
    public Text label;
    public Text value;


    public void setLabel(string l, string v)
    {
        label.text = l;
        value.text = v;
    }
}
