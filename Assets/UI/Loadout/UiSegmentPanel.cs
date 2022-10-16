using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiSegmentPanel : MonoBehaviour
{
    public GameObject statLabelPre;
    public GameObject grid;
    public Text hitType;

    public void addLabel(string label, float value)
    {
        addLabel(label, Power.displayPower(value));
    }
    public void addLabel(string label, string value)
    {
        GameObject l = Instantiate(statLabelPre, grid.transform);
        l.GetComponent<UiStatLabel>().setLabel(label, value);
    }
    public void setType(string type)
    {
        hitType.text = type;
    }
    public void clearLabels()
    {
        foreach (Transform child in grid.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
