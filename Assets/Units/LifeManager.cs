using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class LifeManager : NetworkBehaviour
{
    EventManager events;
    private void Start()
    {
        events = GetComponent<EventManager>();
    }

    bool isDead = false;

    public bool IsDead
    {
        get { return isDead; }
    }



    public void die()
    {
        isDead = true;
        events.fireDeath(true);

        Destroy(gameObject);


    }


}
