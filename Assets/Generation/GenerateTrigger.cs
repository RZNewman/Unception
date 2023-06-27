using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;

public static class GenerateTrigger
{
    public static AttackTrigger generate(float power)
    {
        TriggerConditions conditions = new TriggerConditions();

        float gen;
        conditions.trigger = Trigger.HitRecieved;
        gen = Random.value;
        if (gen < 0.2f)
        {
            conditions.location = AttackSegment.SourceLocation.Body;
        }
        else if (gen < 0.5f)
        {
            conditions.location = AttackSegment.SourceLocation.BodyFixed;
        }
        else
        {
            conditions.location = AttackSegment.SourceLocation.World;
        }


        conditions.recovery = TriggerRecovery.Cooldown;

        AttackBlock b = GenerateAttack.generate(power, AttackGenerationType.PlayerTrigger, 1, null, conditions);
        AttackTrigger trigger = ScriptableObject.CreateInstance<AttackTrigger>();

        trigger.conditions = conditions;
        trigger.block = b;
        trigger.flair = b.flair;
        trigger.id = System.Guid.NewGuid().ToString();

        return trigger;
    }
}
