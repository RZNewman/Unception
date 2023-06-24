using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EventManager;
using static GenerateAttack;

public class TriggerManager : NetworkBehaviour
{
    List<AttackMachine> machines = new List<AttackMachine>();

    // Start is called before the first frame update
    private void Start()
    {


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
        switch (trig.conditions.trigger)
        {
            case Trigger.HitRecieved:
                events.suscribeHit(hitCallback(a));
                break;
        }
    }

    Action abilityCallback(Ability a)
    {
        return () =>
        {
            if (a.ready)
            {
                UnitMovement mover = GetComponent<UnitMovement>();
                AttackMachine m = new AttackMachine(a, mover, false);
                machines.Add(m);
                mover.GetComponent<Cast>().addSource(m);
                //TODO remove machine

            }
        };
    }

    OnHit hitCallback(Ability a)
    {
        Action cast = abilityCallback(a);
        return (GameObject other) =>
        {
            cast();
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
