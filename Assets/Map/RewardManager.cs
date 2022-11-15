using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RewardManager
{
    public readonly static float itemQualityPercent = 1.3f;
    public readonly static float rewardPerDifficulty = 1.3f;

    [Serializable]
    public enum Quality : byte
    {
        Common = 0,
        Uncommon,
        Rare,
        Epic,
        Legendary
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

    public readonly static float itemsPerPack = 2f;

    readonly static int mapsPerFalloff = 16;
    
    readonly static float clearPercent = 0.65f;

    //the percent increase in power to create a scale change equal to the % increase of the highest quality
    //this controls the falloff speed of items during leveling
    //~1.56
    readonly static float powerPercentFalloff = Power.inverseDownscalePower(Power.baseDownscale * itemQualityPercent) / Power.basePower;
    //the XP rate is directly calulated from the desired falloff speed
    public readonly static float powerPackPercent = (powerPercentFalloff - 1) / mapsPerFalloff / Atlas.avgFloorsPerMap / (Atlas.avgPacksPerFloor * clearPercent);

    public static readonly float uncommonChance = 4f / (itemsPerPack * Atlas.avgFloorsPerMap * Atlas.avgPacksPerFloor * clearPercent);
    public static readonly float qualityRarityFactor = 0.25f;

}
