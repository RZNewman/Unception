using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public readonly static float itemQualityPercent = 1.3f;
    public readonly static float rewardPerDifficulty = 1.3f;

    public enum Quality : byte
    {
        Common = 0,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public static float qualityPercent(Quality q)
    {
        return Mathf.Lerp(1, itemQualityPercent, ((int)q) / ((int)Quality.Legendary));
    }

    public readonly static float itemsPerPack = 2f;

    readonly static int mapsPerFalloff = 16;
    readonly static float floorsPerMap = 2f;



    readonly static float powerPercentFalloff = Power.inverseDownscalePower(Power.baseDownscale * itemQualityPercent) / Power.basePower;
    public readonly static float powerPackPercent = (powerPercentFalloff - 1) / mapsPerFalloff / floorsPerMap / MonsterSpawn.packsPerFloor;

    public static readonly float uncommonChance = 4f / (itemsPerPack * floorsPerMap * MonsterSpawn.packsPerFloor);
    public static readonly float qualityRarityFactor = 0.25f;

}