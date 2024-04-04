using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static StatTypes;
using static StatModPanel;

public class UiBuffIcon : MonoBehaviour
{
    public TMP_Text display;
    public TMP_Text charges;
    public Image duration;

    Buff source;
    void setDisplay(string text, Color c)
    {
        display.text = text;
        display.color = c;
    }
    public void setSource(Buff b)
    {
        source = b;
        source.GetComponent<StatHandler>().subscribe();
        IDictionary<Stat, float> stats = source.GetComponent<StatHandler>().stats;
        string label = "X";
        float value = 1;
        if (stats.Count > 0)
        {
            label = statLabel(stats.First().Key);
            value = stats.First().Value;
        }
        setDisplay(label, value >= 0 ? Color.green : Color.red);
    }
    void Update()
    {
        if (source)
        {
            duration.fillAmount = source.progressPercentCountdown;
            charges.text = source.charges;
        }
    }
}
