using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeManager : NetworkBehaviour
{
    GameObject unitBody;

    public delegate void OnDeath();

    List<OnDeath> OnDeathCallbacks = new List<OnDeath>();
    private void Start()
    {
        unitBody = GetComponentInChildren<Size>().gameObject;
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
        foreach(OnDeath c in OnDeathCallbacks)
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
    [ClientRpc]
    void RpcBodyDestroy()
    {
        Destroy(unitBody);
    }
}
