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


    public void addTrigger(List<AttackTrigger> trigs)
    {
        foreach (AttackTrigger trig in trigs)
        {

            instanceTrigger(trig);

        }


    }

    void instanceTrigger(AttackTrigger trig)
    {
        AbiltyManager abiltyManager = GetComponent<AbiltyManager>();

        Ability a = abiltyManager.addAbility(trig.block);
        CastingLocationData castData = new CastingLocationData
        {
            hardCast = false,
            locationOverride = trig.conditions.location,
        };
        switch (trig.conditions.trigger)
        {
            case Trigger.HitRecieved:
                events.suscribeHit(hitCallback(a, castData));
                break;
            case Trigger.Always:
                events.subscribeTransition(alwaysCallback(a, castData));
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
                events.subscribeIndicator(m.indicatorUpdate);
                events.subscribeTick(m.tick);
                events.subscribeTransition(m.transition);

            }
        };
    }

    void removeMachine(AttackMachine m)
    {
        machines.Remove(m);
        events.unsubscribeIndicator(m.indicatorUpdate);
        events.unsubscribeTick(m.tick);
        events.unsubscribeTransition(m.transition);
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
