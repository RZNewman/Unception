using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;
using static CastData;
using static GenerateAttack;
using static GenerateHit;
using static GenerateRepeating;
using static GenerateWind;
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
        instance.effect = populateAttack(effectGeneration, new PopulateAttackOptions
        {
            power = power,
            statLinkAbility = opts.statLinkAbility,
            addedStrength = opts.addedStrength,
            reduceWindValue = opts.reduceWindValue,
        });
        instance.flair = flair;
        instance.id = id;
        instance.scales = scales;
        instance.powerAtGeneration = powerAtGeneration;
        instance.effectGeneration = effectGeneration;
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



}