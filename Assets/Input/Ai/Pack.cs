using System.Collections.Generic;
using UnityEngine;

public class Pack : MonoBehaviour
{
    List<AggroHandler> pack = new List<AggroHandler>();

    public float powerPoolPack;

    public void addToPack(AggroHandler a)
    {
        pack.Add(a);
    }

    public void packAggro(GameObject target)
    {
        target.GetComponentInParent<PackHeal>().addPack(powerPoolPack);
        foreach (AggroHandler a in pack)
        {
            a.addAggro(target);
        }
    }
}
