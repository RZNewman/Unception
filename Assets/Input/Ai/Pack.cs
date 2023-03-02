using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class Pack : NetworkBehaviour
{
    List<GameObject> pack = new List<GameObject>();

    public float powerPoolPack;


    public void addToPack(GameObject u)
    {
        pack.Add(u);
    }
    public void packDeath(GameObject u)
    {
        pack.Remove(u);
    }
    public int unitCount
    {
        get { return pack.Count; }
    }

    public bool packAlive()
    {
        return pack.Count > 0;
    }


    [Server]
    public void disableItemReward()
    {
        foreach (GameObject u in pack)
        {
            u.GetComponent<Reward>().givesItems = false;
        }

    }
    [Server]
    public void disableUnits()
    {
        foreach(GameObject u in pack)
        {
            u.SetActive(false);
        }
        RpcDisablePack(pack);
    }

    [ClientRpc]
    void RpcDisablePack(List<GameObject> packSync)
    {
        if (isClientOnly)
        {
            foreach (GameObject u in packSync)
            {
                u.SetActive(false);
            }
        }
        
    }

    [Server]
    public void enableUnits()
    {
        foreach (GameObject u in pack)
        {
            u.SetActive(true);
        }
        RpcEnablePack(pack);
    }

    [ClientRpc]
    void RpcEnablePack(List<GameObject> packSync)
    {
        if (isClientOnly)
        {
            foreach (GameObject u in packSync)
            {
                u.SetActive(true);
            }
        }
        
    }


    public void packAggro(GameObject target)
    {
        target.GetComponentInParent<PackHeal>().addPack(powerPoolPack);
        foreach (GameObject a in pack)
        {
            a.GetComponent<ControlManager>().spawnControl(); //spawn early so we can access aggro in encoutners
            a.GetComponentInChildren<AggroHandler>().addAggro(target);
        }
    }

    public void reposition(List<Vector3> positions)
    {
        positions.Shuffle();
        for(int i =0; i< pack.Count; i++)
        {
            pack[i].transform.position = positions[i];
        }
    }

    private void OnDestroy()
    {
        foreach (GameObject a in pack)
        {
            Destroy(a);
        }
    }
}
