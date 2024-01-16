using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;
using static CastData;
using static GenerateAttack;
using static GenerateHit;
using static GenerateRepeating;
using static GenerateWind;
using static Power;
using static RewardManager;
using static StatModLabel;
using static StatTypes;

public abstract class AbilityIdentifiers : ScriptableObject
{
    public float powerAtGeneration;
    public bool scales;
    public string id;
    public AttackFlair flair;
}



public abstract class AbilityData : AbilityIdentifiers
{

    public AttackGenerationData effectGeneration;

    protected void populate(AbilityDataInstance instance, FillBlockOptions opts)
    {
        float power = opts.overridePower.HasValue
            && (opts.forceScaling.GetValueOrDefault(false) || scales)
            ? opts.overridePower.Value : powerAtGeneration;

        float numericScale = Power.scaleNumerical(power);

        instance.flair = flair;
        instance.id = id;
        instance.scales = scales;
        instance.powerAtGeneration = powerAtGeneration;
        instance.effectGeneration = effectGeneration;
        instance.powerInstance = power;
        instance.effect = populateAttack(effectGeneration, new PopulateAttackOptions
        {
            power = power,
            scales = new Scales
            {
                numeric = numericScale,
                world = numericScale/opts.baseScales.world,
                time = numericScale/ opts.baseScales.time,
                bases = opts.baseScales,
            },
            multipliedStrength = instance.multipliedStrength(),
            statLinkAbility = opts.statLinkAbility,
            addedStrength = instance.addedStrength(),
            reduceWindValue = opts.reduceWindValue,
        });
    }

    public AbilityDataInstance populate(FillBlockOptions opts)
    {
        return this switch
        {
            CastData c => c.populateCast(opts),
            TriggerData t => t.populateTrigger(opts),
            _ => null
        };
    }


}
#nullable enable
public struct FillBlockOptions
{
    public BaseScales baseScales;
    public float? overridePower;
    public float? addedStrength;
    public bool? reduceWindValue;
    public bool? forceScaling;
    public Ability? statLinkAbility;
}

public abstract class AbilityDataInstance : AbilityIdentifiers
{
    public AttackGenerationData effectGeneration;
    public AttackInstanceData effect;
    public float powerInstance;

    public abstract float actingPower();
 

    public abstract float multipliedStrength();

    public abstract float addedStrength();

    //float modPercentValue
    //{
    //    get
    //    {
    //        return mods == null ? 1 : 1 + mods.Select(m => m.powerPercentValue()).Sum();
    //    }
    //}

}