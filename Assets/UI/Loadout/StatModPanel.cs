using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatModLabel;
using static StatTypes;

public class StatModPanel : MonoBehaviour
{
    public GameObject StatModLabelPre;

    public void fill(AttackBlockInstance block, float playerPower, AttackBlockInstance compare)
    {
        clearLabels();

        List<Stat> labelStats = new List<Stat>() {
            Stat.Haste, Stat.Cooldown, Stat.TurnspeedCast, Stat.MovespeedCast,
            Stat.Charges, Stat.DamageMult,
            Stat.Length, Stat.Width, Stat.Range, Stat.Knockback, Stat.Knockup, Stat.Stagger,

        };
        foreach (Stat stat in labelStats)
        {
            statPopulate(block, stat, playerPower, compare);
        }



    }


    void statPopulate(AttackBlockInstance block, Stat stat, float playerPower, AttackBlockInstance compare)
    {
        StatLabelInfo info = labelInfo(stat, playerPower);
        float value1 = info.valueGetter(block);
        Color color1 = defaultColor;
        string label1 = info.Label;
        float? value2 = info.secondaryGetter(block);
        string label2 = "";
        Color color2 = defaultColor;
        Compare comp = Compare.Neutral;

        if (compare != null)
        {
            comp = compareStat(value1, info.valueGetter(compare), stat);
            color1 = fromCompare(comp);
        }
        if (value2.HasValue)
        {
            label2 = info.LabelSecond + " " + value2.Value;
            if (compare != null)
            {
                Compare comp2 = compareStat(value2.Value, info.secondaryGetter(compare).Value, stat);
                color2 = fromCompare(comp2);
            }
        }

        create()
            .populate(label1, Power.displayPower(value1), block.generationData.getInfo(stat), label2)
            .setColor(color1, color2, comp);

    }

    public enum Compare
    {
        DowngradeLarge,
        DowngradeMid,
        DowngradeSmall,
        Neutral,
        UpgradeSmall,
        UpgradeMid,
        UpgradeLarge,
    }
    public static readonly Color defaultColor = new Color(0.96f, 0.97f, 0.86f);
    public static readonly Color upgradeColor = Color.green;
    public static readonly Color downgradeColor = Color.red;

    float[] thresholds = new float[] { 0.015f, 0.5f, 1.5f };
    float[] colorLerps = new float[] { 0, 0.15f, 0.5f, 1.0f };

    enum ColorCompareFlags
    {
        Reverse,
    }

    Compare compareStat(float a, float b, Stat stat)
    {
        HashSet<ColorCompareFlags> flags = new HashSet<ColorCompareFlags>();
        switch (stat)
        {
            case Stat.Cooldown:
            case Stat.Haste:
                flags.Add(ColorCompareFlags.Reverse);
                break;
        }
        return compareStat(a, b, flags);
    }

    Compare compareStat(float a, float b, HashSet<ColorCompareFlags> flags)
    {
        if (flags.Contains(ColorCompareFlags.Reverse))
        {
            (a, b) = (b, a);
        }
        bool downgrade = b > a;
        float denom = downgrade ? a : b;
        float numer = downgrade ? b : a;
        float proportion;
        if (denom == 0)
        {
            if (numer == 0)
            {
                proportion = 0;
            }
            else
            {
                proportion = 10;
            }
        }
        else
        {
            proportion = (numer / denom) - 1;

        }
        int mod = 0;
        for (int i = 0; i < thresholds.Length; i++)
        {
            if (proportion > thresholds[i])
            {
                mod++;
            }
        }
        return (Compare)((downgrade ? -1 : 1) * mod + (int)(Compare.Neutral));

    }
    Color fromCompare(Compare comp)
    {
        int value = ((int)comp) - ((int)Compare.Neutral);
        int mag = Mathf.Abs(value);
        bool downgrade = value < 0;
        return Color.Lerp(defaultColor, downgrade ? downgradeColor : upgradeColor, colorLerps[mag]);
    }

    struct StatLabelInfo
    {
        public string Label;
        public Func<AttackBlockInstance, float> valueGetter;
        public string LabelSecond;
        public Func<AttackBlockInstance, float?> secondaryGetter;
    }
    StatLabelInfo labelInfo(Stat stat, float playerPower)
    {
        string label = "";
        string labelSecond = "";
        Func<AttackBlockInstance, float> valueGetter = x => 0;
        Func<AttackBlockInstance, float?> secondaryGetter = x => null;
        switch (stat)
        {
            case Stat.DamageMult:
                label = "DPS";
                valueGetter = b => b.instance.dps(playerPower);
                labelSecond = "Damage";
                secondaryGetter = b => b.instance.damage(playerPower);
                break;
            case Stat.Cooldown:
                label = "CD";
                valueGetter = b => b.instance.cooldownDisplay(playerPower);
                break;
            case Stat.Charges:
                label = "Charges";
                valueGetter = b => b.instance.getCharges();
                break;
            case Stat.Haste:
                label = "Cast";
                valueGetter = b => b.instance.castTimeDisplay(playerPower);
                break;
            case Stat.TurnspeedCast:
                label = "Turn";
                valueGetter = b => b.instance.avgTurn();
                break;
            case Stat.MovespeedCast:
                label = "Move";
                valueGetter = b => b.instance.avgMove();
                break;
            case Stat.Length:
                label = "Length";
                valueGetter = b => b.instance.avgLength();
                break;
            case Stat.Width:
                label = "Width";
                valueGetter = b => b.instance.avgWidth();
                break;
            case Stat.Range:
                label = "Range";
                valueGetter = b => b.instance.avgRange();
                break;
            case Stat.Knockback:
                label = "Knockback";
                valueGetter = b => b.instance.avgKback();
                break;
            case Stat.Knockup:
                label = "Knockup";
                valueGetter = b => b.instance.avgKup();
                break;
            case Stat.Stagger:
                label = "Stagger";
                valueGetter = b => b.instance.avgStagger();
                break;

        }
        return new StatLabelInfo
        {
            Label = label,
            valueGetter = valueGetter,
            LabelSecond = labelSecond,
            secondaryGetter = secondaryGetter,
        };
    }



    StatModLabel create()
    {
        return Instantiate(StatModLabelPre, transform).GetComponent<StatModLabel>();
    }
    public void clearLabels()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
