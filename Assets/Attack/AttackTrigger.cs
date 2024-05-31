using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackSegment;
using static AttackUtils;
using static GenerateAttack;

public enum Trigger
{
    Always,
    Cast,
    HitRecieved,
    HitGiven
}
public enum TriggerRecovery
{
    Cooldown,
    //CastCount,
}


public struct TriggerConditions
{
    public Trigger trigger;
    public ItemSlot? triggerSlot;
    public TriggerRecovery recovery;
    public SourceLocation location;

    static readonly float triggerBaseStrength = 1.5f;

    public float triggerStrength
    {
        get
        {
            float strength = triggerBaseStrength;
            if (triggerSlot.HasValue)
            {
                strength *= 1.25f;
            }
            return strength;
        }
    }
}

public class TriggerData : AbilityData
{
    public TriggerConditions conditions;
    public float difficultyTotal;

    public TriggerDataInstance populateTrigger(FillBlockOptions opts)
    {

        TriggerDataInstance filled = ScriptableObject.CreateInstance<TriggerDataInstance>();
        opts.reduceWindValue = true;
        filled.conditions = conditions;
        filled.difficultyTotal = difficultyTotal;
        populate(filled, opts);
        return filled;
    }

}

public class TriggerDataInstance : AbilityDataInstance
{
    public TriggerConditions conditions;
    public float difficultyTotal;

    public override StrengthMultiplers strength()
    {
        return new StrengthMultiplers(conditions.triggerStrength, 1 + difficultyTotal * 0.25f);
    }

    public override float actingPower()
    {
        return powerInstance * strength();
    }
}


