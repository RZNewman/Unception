using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateBuff;

public class BuffManager : NetworkBehaviour
{
    public delegate void BuffRefresh(List<Buff> b);


    StatHandler handler;
    Power power;
    void Start()
    {
        handler = GetComponent<StatHandler>();
        power = GetComponentInParent<Power>();
    }

    void debugStats(List<Buff> b)
    {
        handler.debugStats();
    }


    List<Buff> buffs = new List<Buff>();
    List<BuffRefresh> subs = new List<BuffRefresh>();

    public void addBuff(Buff b)
    {
        buffs.Add(b);
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
