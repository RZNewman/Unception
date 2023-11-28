using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static GenerateHit;
using static GenerateWind;
using static GenerateDash;
using static AttackUtils;
using static StatTypes;
using static RewardManager;
using static GroveObject;

public class CastDataInstance : AbilityDataInstance
{
    public ItemSlot? slot;
    public Quality quality;
    public int stars;
    public GroveShape shape;

    public override float enhancementStrength()
    {
        return qualityPercent(quality) + 0.03f * stars;
    }

}

