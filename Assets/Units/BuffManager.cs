using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    public delegate void BuffRefresh(List<Buff> b);



    List<Buff> buffs = new List<Buff>();
    List<BuffRefresh> subs = new List<BuffRefresh>();

    public void addBuff(Buff b)
    {
        buffs.Add(b);
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
