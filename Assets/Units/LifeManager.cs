using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class LifeManager : NetworkBehaviour
{
    GameObject unitBody
    {
        get
        {
            return GetComponentInChildren<Size>().gameObject;
        }
    }

    public delegate void OnDeath();

    List<OnDeath> OnDeathCallbacks = new List<OnDeath>();

    private void Start()
    {
    }
    bool isPlayer
    {
        get
        {
            return GetComponent<ControlManager>().IsPlayer;
        }
    }
    bool isDead = false;

    public bool IsDead
    {
        get { return isDead; }
    }

    public void suscribeDeath(OnDeath d)
    {
        OnDeathCallbacks.Add(d);
    }

    public void die()
    {
        isDead = true;
        foreach (OnDeath c in OnDeathCallbacks)
        {
            c();
        }
        if (!isPlayer)
        {
            Destroy(gameObject);
        }
        else
        {
            Destroy(unitBody);
            RpcBodyDestroy();
        }

    }
    private void OnDestroy()
    {
        if (!isDead)
        {
            foreach (OnDeath c in OnDeathCallbacks)
            {
                c();
            }
        }

    }
    [ClientRpc]
    void RpcBodyDestroy()
    {
        if (isClientOnly)
        {
            Destroy(unitBody);
        }

    }
}
