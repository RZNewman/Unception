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

    // Start is called before the first frame update
    private void Start()
    {
        combat = GetComponent<Combat>();

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
        EventManager events = GetComponent<EventManager>();
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
        }
    }

    Action<CastingLocationData> abilityCallback(Ability a)
    {
        return (CastingLocationData castData) =>
        {
            if (a.ready)
            {
                UnitMovement mover = GetComponent<UnitMovement>();

                AttackMachine m = new AttackMachine(a, mover, castData);
                machines.Add(m);
                mover.GetComponent<Cast>().addSource(m, removeMachine);

            }
        };
    }

    void removeMachine(AttackMachine m)
    {
        machines.Remove(m);
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

    [ClientRpc]
    void RpcBuildTrigger(TriggerConditions cond, Ability a)
    {
        if (isServer)
        {
            return;
        }

    }
}
