using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static EventManager;

public class Breakable : NetworkBehaviour, TeamOwnership
{
    public GameObject UrnPre;
    public GameObject ChestPre;
    Reward r;
    LifeManager life;
    EventManager events;
    SoundManager sound;


    public enum BreakableType
    {
        Urn,
        Chest
    }
    public static int numberBreakables(BreakableType type)
    {
        switch (type)
        {
            case BreakableType.Urn:
                return Random.Range(3, 6);
            default:
                return 1;
        }
    }
    public static float packpercent(BreakableType type)
    {
        switch (type)
        {
            case BreakableType.Urn:
                return 0.5f;
            case BreakableType.Chest:
                return 5f;
            default:
                return 0.5f;
        }
    }
    public static float qualityMult(BreakableType type)
    {
        switch (type)
        {
            case BreakableType.Urn:
                return 1;
            case BreakableType.Chest:
                return 2;
            default:
                return 1;
        }
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
        events = GetComponent<EventManager>();
        instanceBody();
        if (isServer)
        {

            events.HitEvent += onHit;
            events.suscribeDeath(onDeath);
        }
    }

    void onHit(GetHitEventData data)
    {
        data.other.GetComponent<Reward>().recieveReward(r);
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
