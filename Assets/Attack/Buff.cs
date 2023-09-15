using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
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

    [SyncVar]
    ItemSlot? slot;

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
            EventManager em = transform.GetComponentInParent<EventManager>();
            if (em)
            {
                em.CastEvent -= OnCast;
            }

        }
    }

    void OnCast(Ability a)
    {
        ItemSlot? castSlot = a.slot();
        if (castSlot.HasValue)
        {
            if (!slot.HasValue || slot.Value == castSlot.Value)
            {
                castCount--;
            }

        }

    }
    public void setup(BuffInstanceData buff)
    {
        durationMax = buff.durration;
        duration = buff.durration;
        timeScale = Power.scaleTime(buff.powerAtGen);
        castCount = buff.castCount;
        slot = buff.slot;
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
