using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackSegment;
using static GenerateAttack;
using static Utils;

public static class GenerateTrigger
{
    public static AttackTrigger generate(float power)
    {
        TriggerConditions conditions = new TriggerConditions();

        float gen;
        gen = Random.value;
        if (gen < 0.2f)
        {
            conditions.trigger = Trigger.Always;
        }
        else if (gen < 0.3f)
        {
            conditions.trigger = Trigger.HitRecieved;
        }
        else
        {
            conditions.trigger = Trigger.Cast;
            gen = Random.value;
            if (gen > 0.3f)
            {
                conditions.triggerSlot = new List<ItemSlot>(EnumValues<ItemSlot>()).RandomItem();
            }
        }

        gen = Random.value;
        conditions.location = conditions.trigger switch
        {
            Trigger.Always => SourceLocation.Body,
            Trigger.HitRecieved => gen switch
            {
                < 0.2f => SourceLocation.Body,
                < 0.5f => SourceLocation.BodyFixed,
                _ => SourceLocation.World,
            },
            Trigger.Cast => gen switch
            {
                < 0.5f => SourceLocation.Body,
                _ => SourceLocation.WorldForward,
            },
            _ => SourceLocation.Body
        };


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
