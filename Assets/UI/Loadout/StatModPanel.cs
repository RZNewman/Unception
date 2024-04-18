using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static StatModLabel;
using static StatTypes;

public class StatModPanel : MonoBehaviour
{
    public GameObject StatModLabelPre;


    public void fill(AbilityDataInstance block, float playerPower, AbilityDataInstance compare = null)
    {
        clearLabels();

        List<Stat> labelStats = new List<Stat>() {
            Stat.Haste, Stat.Cooldown, Stat.TurnspeedCast, Stat.MovespeedCast,
            Stat.Charges, Stat.DamageMult,
            Stat.Length, Stat.Width, Stat.Range, Stat.Knockback, Stat.Knockup, Stat.Stagger,  Stat.Mezmerize,

        };
        foreach (Stat stat in labelStats)
        {
            statPopulate(block, stat, playerPower, compare);
        }



    }


    void statPopulate(AbilityDataInstance block, Stat stat, float playerPower, AbilityDataInstance compare)
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
            .populate(label1, Power.displayPower(value1), block.effectGeneration.getInfo(stat), label2)
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
        public Func<AbilityDataInstance, float> valueGetter;
        public string LabelSecond;
        public Func<AbilityDataInstance, float?> secondaryGetter;
    }
    static StatLabelInfo labelInfo(Stat stat, float playerPower)
    {
        string label = statLabel(stat);
        string labelSecond = "";
        Func<AbilityDataInstance, float> valueGetter = x => 0;
        Func<AbilityDataInstance, float?> secondaryGetter = x => null;
        switch (stat)
        {
            case Stat.DamageMult:
                label = "DPS";
                valueGetter = a => a.effect.dps(playerPower);
                labelSecond = "Damage";
                secondaryGetter = a => a.effect.damage(playerPower);
                break;
            case Stat.Cooldown:
                valueGetter = a => a.effect.cooldownDisplay(playerPower);
                break;
            case Stat.Charges:
                valueGetter = a => a.effect.getCharges();
                break;
            case Stat.Haste:
                valueGetter = a => a.effect.castTimeDisplay(playerPower);
                break;
            case Stat.TurnspeedCast:
                valueGetter = a => a.effect.avgTurn();
                break;
            case Stat.MovespeedCast:
                valueGetter = a => a.effect.avgMove();
                break;
            case Stat.Length:
                valueGetter = a => a.effect.avgLength();
                break;
            case Stat.Width:
                valueGetter = a => a.effect.avgWidth();
                break;
            case Stat.Range:
                valueGetter = a => a.effect.avgRange();
                break;
            case Stat.Knockback:
                valueGetter = a => a.effect.avgKback();
                break;
            case Stat.Knockup:
                valueGetter = a => a.effect.avgKup();
                break;
            case Stat.Stagger:
                valueGetter = a => a.effect.avgStagger();
                break;
            case Stat.Mezmerize:
                valueGetter = a => a.effect.avgMezmerize();
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

    public static string statLabel(Stat stat)
    {
        switch (stat)
        {
            case Stat.Cooldown:
                return "Cooldown";
            case Stat.Charges:
                return "Charges";
            case Stat.Haste:
                return "Cast speed";
            case Stat.TurnspeedCast:
            case Stat.Turnspeed:
                return "Turn speed";
            case Stat.MovespeedCast:
            case Stat.Movespeed:
                return "Move speed";
            case Stat.Length:
                return "Length";
            case Stat.Width:
                return "Width";
            case Stat.Range:
                return "Range";
            case Stat.Knockback:
                return "Knockback";
            case Stat.Knockup:
                return "Knockup";
            case Stat.Stagger:
                return "Stagger";
            case Stat.Mezmerize:
                return "Stun";
            default:
                return "UNK";

        }
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
