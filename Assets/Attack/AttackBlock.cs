using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static GenerateHit;
using static GenerateRepeating;
using static GenerateWind;
using static Grove;
using static GroveObject;
using static RewardManager;
using static StatModLabel;
using static StatTypes;

public class CastData : AbilityData
{
    public ItemSlot? slot;
    public Quality quality;
    public int stars;
    public GroveShape shape;

    public CastDataInstance populateCast(FillBlockOptions opts)
    {

        CastDataInstance filled = ScriptableObject.CreateInstance<CastDataInstance>();
        filled.slot = slot;
        filled.quality = quality;
        filled.stars = stars;
        filled.shape = shape;
        populate(filled, opts);
        return filled;
    }

}
