using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using static EventManager;
using static GenerateBuff;

public class BuffManager : NetworkBehaviour
{
    public delegate void BuffRefresh(List<Buff> b);


    StatHandler handler;
    Power power;
    EventManager events;
    void Start()
    {
        handler = GetComponent<StatHandler>();
        power = GetComponentInParent<Power>();
        events = transform.GetComponentInParent<EventManager>();
    }

    void debugStats(List<Buff> b)
    {
        handler.debugStats();
    }

    public EventManager eventManager
    {
        get { return events; }
    }

    List<Buff> buffs = new List<Buff>();
    List<BuffRefresh> subs = new List<BuffRefresh>();

    public void addBuff(Buff b)
    {
        buffs.Add(b);
        b.setManager(this);
        events.TickEvent += b.Tick;
        events.CastEvent += b.OnCast;
        if (isServer)
        {
            switch (b.buffMode)
            {
                case BuffMode.Dot:
                case BuffMode.Shield:
                    GetComponent<Health>().addReference(b);
                    break;
                case BuffMode.Timed:
                case BuffMode.Cast:
                    b.GetComponent<StatHandler>().link(handler, b.relativeScale(power.scaleTime()));
                    break;
            }

        }
        callback();
    }
    public void removeBuff(Buff b)
    {
        buffs.Remove(b);
        EventManager events = transform.GetComponentInParent<EventManager>();
        events.TickEvent -= b.Tick;
        events.CastEvent -= b.OnCast;
        if (isServer)
        {
            switch (b.buffMode)
            {
                case BuffMode.Dot:
                case BuffMode.Shield:
                    GetComponent<Health>().removeReference(b);
                    break;
                    //Stat stream unlink is in StatHandler OnDestroy
            }

        }
        callback();
    }

    public void subscribe(BuffRefresh br)
    {
        subs.Add(br);
        br(buffs);
    }

    void callback()
    {
        foreach (BuffRefresh br in subs)
        {
            br(buffs);
        }
    }
}
