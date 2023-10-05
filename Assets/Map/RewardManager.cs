using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RewardManager
{
    //This also increases the XP rate propotionally, bc it increases the falloff rate
    public readonly static float itemQualityPercent = 1.5f;
    public readonly static float bonusRewardPerDifficulty = 0.3f;

    [Serializable]
    public enum Quality : byte
    {
        Common = 0,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    [Serializable]
    public enum ModCount : byte
    {
        Zero = 0,
        One,
        Two,
        Three,
        Four,
        Five,
    }
    [Serializable]
    public enum ModBonus : byte
    {
        Zero = 0,
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten
    }

    public static int count(this ModCount mc)
    {
        return (int)mc;
    }
    public static string qualitySymbol(Quality q)
    {
        switch (q)
        {
            case Quality.Common:
                return "C";
            case Quality.Uncommon:
                return "U";
            case Quality.Rare:
                return "R";
            case Quality.Epic:
                return "E";
            case Quality.Legendary:
                return "L";
            default:
                return "";
        }
    }
    public static Color colorQuality(Quality q)
    {
        switch (q)
        {
            case Quality.Common:
                return GameColors.QualityCommon;
            case Quality.Uncommon:
                return GameColors.QualityUncommon;
            case Quality.Rare:
                return GameColors.QualityRare;
            case Quality.Epic:
                return GameColors.QualityEpic;
            case Quality.Legendary:
                return GameColors.QualityLegendary;
            default: return Color.white;
        }
    }

    public static float qualityPercent(Quality q)
    {
        return Mathf.Lerp(1, itemQualityPercent, ((float)q) / ((float)Quality.Legendary));
    }

    public readonly static float itemsPerPack = 3f;

    readonly static float mapsPerFalloff = 3.0f;

    //the percent increase in power to create a scale change equal to the % increase of the highest quality
    //this controls the falloff speed of items during leveling
    //~1.56
    readonly static float powerPercentFalloff = Power.inverseDownscalePower(Power.baseDownscale * itemQualityPercent) / Power.basePower;
    //the XP rate is directly calulated from the desired falloff speed
    public readonly static float packsKilledPerMap = 12f;


    public readonly static float powerMapPercent = (powerPercentFalloff - 1) / mapsPerFalloff;
    public readonly static float powerPackPercent = powerMapPercent / packsKilledPerMap;


    public static readonly float uncommonChance = 4f / (itemsPerPack * packsKilledPerMap);
    public static readonly float qualityRarityFactor = 0.25f;

    public static readonly float chestChance = 0.125f;

    public static readonly float oneModChance = uncommonChance;
    public static readonly float modCountRarityFactor = 0.25f;

    public static readonly float modBonusChance = 0.05f;
    public static readonly float modBonusRarityFactor = 0.5f;



}
