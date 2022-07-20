using System.Collections.Generic;
using UnityEngine;

public class Pack : MonoBehaviour
{
    List<AggroHandler> pack = new List<AggroHandler>();

    public void addToPack(AggroHandler a)
    {
        pack.Add(a);
    }

    public void packAggro(GameObject target)
    {
        foreach (AggroHandler a in pack)
        {
            a.addAggro(target);
        }
    }
}
