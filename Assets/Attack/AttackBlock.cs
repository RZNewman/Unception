using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static GenerateHit;
using static GenerateRepeating;
using static GenerateWind;
using static StatModLabel;
using static StatTypes;

public class CastData : AbilityData
{
    public ItemSlot? slot;

    public CastDataInstance populateCast(FillBlockOptions opts)
    {

        CastDataInstance filled = ScriptableObject.CreateInstance<CastDataInstance>();
        populate(filled, opts);
        filled.slot = slot;
        return filled;
    }

}
