using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Atlas;

public class UiMapDetails : MonoBehaviour
{
    public GameObject statLabelPre;


    public void setMapDetails(Map m)
    {
        clearLabels();
        if (m.quest)
        {
            addLabel("Quest Tier", m.tier);
        }
        addLabel("Power", Power.displayExaggertatedPower(m.power));
        addLabel("Floors", m.floors.Length);
        addLabel("Encounters", m.floors.Sum(f => f.encounters.Length));
        addLabel("Difficulty", (m.difficulty.total + 1).asPercent());
        addLabel("Pack size", (m.difficulty.pack + 1).asPercent());
        addLabel("Veterans", (m.difficulty.veteran).asPercent());
        addLabel("Champions", (m.difficulty.champion).asPercent());
    }






    void addLabel(string label, float value)
    {
        addLabel(label, value.ToString());
    }
    void addLabel(string label, string value)
    {
        GameObject l = Instantiate(statLabelPre, transform);
        l.GetComponent<UiStatLabel>().setLabel(label, value);
    }

    public void clearLabels()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
