using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : NetworkBehaviour
{
    public delegate void BuffRefresh(List<Buff> b);


    StatHandler handler;
    Power power;
    void Start()
    {
        handler = GetComponent<StatHandler>();
        power = GetComponent<Power>();
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
            b.GetComponent<StatHandler>().link(handler, b.relativeScale(power.scaleTime()));
        }
        callback();
    }
    public void removeBuff(Buff b)
    {
        buffs.Remove(b);
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
