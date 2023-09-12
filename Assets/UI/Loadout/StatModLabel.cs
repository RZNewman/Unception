using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static StatModPanel;

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

    public Image CompareArrow;

    public struct StatInfo
    {
        public float maxStat;
        public float maxRoll;
        public float percentRoll;
        public float modPercent;
        public float moddedStat;
        public Color fill;
    }

    public StatModLabel populate(string title, string value, StatInfo i, string secondary)
    {
        Label.text = title;
        Value.text = value;
        Secondary.text = secondary;
        ModCount.text = Mathf.Round(i.modPercent * 100).ToString();

        float width = StatBarBase.sizeDelta.x;
        //float maxRollPercent = i.maxRoll / i.maxStat;
        MaxRoll.sizeDelta = new Vector2(0, MaxRoll.sizeDelta.y);
        RollBar.sizeDelta = new Vector2(i.percentRoll * width, RollBar.sizeDelta.y);
        RollBar.GetComponent<Image>().color = i.fill;
        ModBar.sizeDelta = new Vector2(i.moddedStat / i.maxStat * width, ModBar.sizeDelta.y);

        if (i.moddedStat <= 0)
        {
            ModdedVisual.SetActive(false);
        }

        CompareArrow.gameObject.SetActive(false);
        return this;
    }


    public void setColor(Color a, Color b, Compare compare)
    {
        Value.color = a;
        Secondary.color = b;
        CompareArrow.color = a;
        if (compare != Compare.Neutral)
        {
            CompareArrow.gameObject.SetActive(true);
            float value = ((int)compare) - ((int)Compare.Neutral);
            value /= 2.0f;
            bool downgrade = value < 0;
            value = Mathf.Abs(value);
            CompareArrow.rectTransform.sizeDelta *= new Vector2(value, value);
            if (downgrade)
            {
                CompareArrow.rectTransform.rotation *= Quaternion.Euler(0, 0, 180);
            }

        }

    }

}
