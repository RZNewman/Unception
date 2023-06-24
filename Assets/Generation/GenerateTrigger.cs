using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;

public static class GenerateTrigger
{
    public static AttackTrigger generate(float power)
    {
        TriggerConditions conditions = new TriggerConditions();

        conditions.trigger = Trigger.HitRecieved;
        conditions.location = AttackSegment.SourceLocation.Body;
        conditions.recovery = TriggerRecovery.Cooldown;

        AttackBlock b = GenerateAttack.generate(power, AttackGenerationType.PlayerTrigger);
        AttackTrigger trigger = ScriptableObject.CreateInstance<AttackTrigger>();

        trigger.conditions = conditions;
        trigger.block = b;
        trigger.flair = b.flair;
        trigger.id = System.Guid.NewGuid().ToString();

        return trigger;
    }
}
