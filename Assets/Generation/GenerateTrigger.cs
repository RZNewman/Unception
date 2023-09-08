using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackSegment;
using static GenerateAttack;
using static Utils;

public static class GenerateTrigger
{
    public static TriggerData generate(float power)
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

        AttackGenerationData a = GenerateAttack.generateAttack(null, AttackGenerationType.PlayerTrigger, 1, null, conditions);
        TriggerData trigger = ScriptableObject.CreateInstance<TriggerData>();

        trigger.conditions = conditions;
        trigger.effectGeneration = a;
        trigger.flair = generateFlair();
        trigger.powerAtGeneration = power;
        trigger.id = System.Guid.NewGuid().ToString();

        return trigger;
    }
}
