using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Breakable : NetworkBehaviour, TeamOwnership
{
    public GameObject UrnPre;
    public GameObject ChestPre;
    Reward r;
    LifeManager life;
    SoundManager sound;


    public enum BreakableType
    {
        Urn,
        Chest
    }
    [SyncVar]
    public BreakableType type;


    void instanceBody()
    {
        GameObject pre;
        switch (type)
        {
            case BreakableType.Urn:
                pre = UrnPre;
                break;
            case BreakableType.Chest:
                pre = ChestPre;
                break;
            default:
                pre = UrnPre;
                break;
        }
        Instantiate(pre, transform);
    }
    public uint getTeam()
    {
        return 0;//default enemy team
    }

    // Start is called before the first frame update
    void Start()
    {
        r = GetComponent<Reward>();
        life = GetComponent<LifeManager>();
        sound = FindObjectOfType<SoundManager>();
        instanceBody();
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

    void onDeath(bool natural)
    {
        if (!natural)
        {
            return;
        }
        SoundManager.SoundClip clip;
        switch (type)
        {
            case BreakableType.Urn:
                clip = SoundManager.SoundClip.Shatter;
                break;
            case BreakableType.Chest:
                clip = SoundManager.SoundClip.Creak;
                break;
            default:
                clip = SoundManager.SoundClip.Shatter;
                break;
        }
        sound.sendSound(clip, transform.position);
    }


}
