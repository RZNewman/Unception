using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateBuff;
using static StatTypes;

public class Buff : NetworkBehaviour
{
    [SyncVar]
    float duration;

    [SyncVar]
    float durationMax;

    [SyncVar]
    float castCount;

    BuffMode mode = BuffMode.Timed;

    float timeScale;
    EventManager events;

    public float relativeScale(float targetTimeScale)
    {
        return timeScale / targetTimeScale;
    }

    public float remainingPercent
    {
        get
        {
            return duration / durationMax;
        }
    }
    public string charges
    {
        get
        {
            if (mode == BuffMode.Cast)
            {
                return castCount.ToString();
            }
            else
            {
                return "";
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<ClientAdoption>().trySetAdopted();
        transform.parent.GetComponent<BuffManager>().addBuff(this);
        events = transform.GetComponentInParent<EventManager>();
        events.TickEvent += Tick;
        if (mode == BuffMode.Cast)
        {
            events.CastEvent += OnCast;
        }
    }


    private void OnDestroy()
    {
        events.TickEvent -= Tick;
        if (mode == BuffMode.Cast)
        {
            transform.GetComponentInParent<EventManager>().CastEvent -= OnCast;
        }
    }

    void OnCast(Ability a)
    {
        if (a.source().slot.HasValue)
        {
            castCount--;
        }

    }
    public void setup(float durr, float casts, float timeS)
    {
        durationMax = durr;
        duration = durr;
        timeScale = timeS;
        castCount = casts;
        if (castCount > 0)
        {
            mode = BuffMode.Cast;
        }
    }

    // Update is called once per frame
    void Tick()
    {
        duration -= Time.fixedDeltaTime;
        if (isServer)
        {
            if (duration <= 0 || (mode == BuffMode.Cast && castCount <= 0))
            {
                transform.parent.GetComponent<BuffManager>().removeBuff(this);
                Destroy(gameObject);
            }

        }
    }
}
