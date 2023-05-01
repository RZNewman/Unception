using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatModLabel : MonoBehaviour
{
    public RectTransform StatBarBase;
    public RectTransform MaxRoll;
    public RectTransform RollBar;
    public RectTransform ModBar;

    public GameObject ModdedVisual;

    public TMP_Text Label;
    public TMP_Text Value;
    public TMP_Text Secondary;
    public TMP_Text ModCount;

    public struct StatInfo
    {
        public float maxStat;
        public float maxRoll;
        public float percentRoll;
        public float modPercent;
        public float moddedStat;
        public Color fill;
    }

    public void populate(string title, string value, StatInfo i, string secondary = "")
    {
        Label.text = title;
        Value.text = value;
        Secondary.text = secondary;
        ModCount.text = Mathf.Round(i.modPercent * 100).ToString();

        float width = StatBarBase.sizeDelta.x;
        float maxRollPercent = i.maxRoll / i.maxStat;
        MaxRoll.sizeDelta = new Vector2(maxRollPercent * width, MaxRoll.sizeDelta.y);
        RollBar.sizeDelta = new Vector2(i.percentRoll * maxRollPercent * width, RollBar.sizeDelta.y);
        RollBar.GetComponent<Image>().color = i.fill;
        ModBar.sizeDelta = new Vector2(i.moddedStat / i.maxStat * width, ModBar.sizeDelta.y);

        if(i.moddedStat <= 0)
        {
            ModdedVisual.SetActive(false);
        }
    }

}
