using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Breakable : NetworkBehaviour, TeamOwnership
{
    Reward r;
    LifeManager life;
    public uint getTeam()
    {
        return 0;//default enemy team
    }

    // Start is called before the first frame update
    void Start()
    {
        r = GetComponent<Reward>();
        life = GetComponent<LifeManager>();
        if (isServer)
        {

            life.suscribeHit(onHit);
            life.suscribeDeath(onDeath);
        }
    }

    void onHit(GameObject other)
    {
        other.GetComponent<Reward>().recieveReward(r);
        life.die();
    }

    void onDeath()
    {
        Destroy(gameObject);
    }


}
