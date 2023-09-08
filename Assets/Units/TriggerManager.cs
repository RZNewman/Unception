using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackMachine;
using static AttackSegment;
using static EventManager;
using static GenerateAttack;

public class TriggerManager : NetworkBehaviour
{
    List<AttackMachine> machines = new List<AttackMachine>();
    Combat combat;
    EventManager events;

    // Start is called before the first frame update
    private void Awake()
    {
        combat = GetComponent<Combat>();
        events = GetComponent<EventManager>();
    }


    public void addTrigger(List<TriggerData> trigs)
    {
        foreach (TriggerData trig in trigs)
        {

            instanceTrigger(trig);

        }


    }


    void instanceTrigger(TriggerData trig)
    {
        AbiltyManager abiltyManager = GetComponent<AbiltyManager>();

        Ability a = abiltyManager.addTriggeredAbility(trig, trig.conditions.triggerStrength);
        CastingLocationData castData = new CastingLocationData
        {
            hardCast = false,
            locationOverride = trig.conditions.location,
        };
        switch (trig.conditions.trigger)
        {
            case Trigger.HitRecieved:
                events.HitEvent += (hitCallback(a, castData));
                break;
            case Trigger.Always:
                events.TransitionEvent += (alwaysCallback(a, castData));
                break;
            case Trigger.Cast:
                events.CastEvent += (castCallback(a, castData, trig.conditions.triggerSlot));
                break;
        }
    }

    Action<CastingLocationData> abilityCallback(Ability a)
    {
        return (CastingLocationData castData) =>
        {
            if (a.ready)
            {
                UnitMovement mover = GetComponent<UnitMovement>();

                AttackMachine m = new AttackMachine(a, mover, castData, removeMachine);
                machines.Add(m);
                events.IndicatorEvent += (m.indicatorUpdate);
                events.TickEvent += (m.tick);
                events.TransitionEvent += (m.transition);

            }
        };
    }

    void removeMachine(AttackMachine m)
    {
        machines.Remove(m);
        events.IndicatorEvent -= (m.indicatorUpdate);
        events.TickEvent -= (m.tick);
        events.TransitionEvent -= (m.transition);
    }
    private void OnDestroy()
    {
        List<AttackMachine> toRemove = new List<AttackMachine>(machines);
        toRemove.ForEach(m =>
        {
            m.exit();
        });
    }

    OnHit hitCallback(Ability a, CastingLocationData location)
    {
        Action<CastingLocationData> cast = abilityCallback(a);
        return (GameObject other) =>
        {
            location.triggeredPosition = other.transform.position;
            cast(location);
        };
    }

    OnCast castCallback(Ability a, CastingLocationData location, ItemSlot? slotRestriction)
    {
        Action<CastingLocationData> cast = abilityCallback(a);
        return (Ability castAbil) =>
        {
            //only player cast abilities
            if (castAbil.slot().HasValue
            && (
            !slotRestriction.HasValue || castAbil.slot().Value == slotRestriction.Value
            )
            )
            {
                cast(location);
            }

        };
    }

    OnTransition alwaysCallback(Ability a, CastingLocationData location)
    {
        Action<CastingLocationData> cast = abilityCallback(a);
        return () =>
        {
            if (combat.inCombat)
            {
                cast(location);
            }

        };
    }

    //TODO client trigger data
    [ClientRpc]
    void RpcBuildTrigger(TriggerConditions cond, Ability a)
    {
        if (isServer)
        {
            return;
        }

    }
}
