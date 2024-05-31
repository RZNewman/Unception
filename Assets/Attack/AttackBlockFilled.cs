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
using static Grove;

public class CastDataInstance : AbilityDataInstance
{
    public ItemSlot? slot;
    public Quality quality;
    public int stars;
    public GroveShape shape;

    static readonly Dictionary<GroveSlotType, float> shapeValues = new Dictionary<GroveSlotType, float>()
    {
        { GroveSlotType.Hard,0.15f },
        { GroveSlotType.Aura,0.07f },
    };
    public override StrengthMultiplers strength()
    {
        return new StrengthMultiplers(shape.strength(shapeValues), multipliedStrength());
    }

    public float multipliedStrength()
    {
        return qualityPercent(quality) + 0.03f * stars;
    }

    public override float actingPower()
    {
        return powerInstance * multipliedStrength();
    }
}

