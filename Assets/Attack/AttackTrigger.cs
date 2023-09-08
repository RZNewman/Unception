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
        populate(filled, opts);
        filled.conditions = conditions;
        filled.difficultyTotal = difficultyTotal;
        return filled;
    }

}

public class TriggerDataInstance : AbilityDataInstance
{
    public TriggerConditions conditions;
    public float difficultyTotal;

    public override float enhancementStrength()
    {
        return 1 + difficultyTotal * 0.25f;
    }


}


