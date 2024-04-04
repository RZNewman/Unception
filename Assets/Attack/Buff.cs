using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EventManager;
using static GenerateAttack;
using static GenerateBuff;
using static GenerateHit.HitInstanceData;
using static StatTypes;

public class Buff : NetworkBehaviour
{
    [SyncVar]
    float durationRemaining;

    [SyncVar]
    float durationMax;

    [SyncVar]
    float castCount;

    [SyncVar]
    ItemSlot? slot;

    [SyncVar]
    float value;

    [SyncVar]
    float valueMax;

    [SyncVar]
    float regen;

    [SyncVar]
    HarmValues harm;

    BuffMode mode = BuffMode.Timed;

    float timeScale;
    EventManager events;
    public float relativeScale(float targetTimeScale)
    {
        return timeScale / targetTimeScale;
    }

    public float remainingDuration
    {
        get
        {
            return durationRemaining;
        }
    }

    public float progressPercentCountdown
    {
        get
        {
            return durationRemaining / durationMax;
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

        transform.parent.GetComponent<BuffManager>().removeBuff(this);

        events.TickEvent -= Tick;
        if (mode == BuffMode.Cast)
        {
            events.CastEvent -= OnCast;
        }
    }

    public float damageRemaining
    {
        get
        {
            //Debug.Log(harm.totalDamage + " - " + progressPercent);
            return harm.totalDamage * progressPercentCountdown;
        }
    }

    public float valueCurrent
    {
        get
        {
            return value;
        }
    }
    public BuffMode buffMode
    {
        get
        {
            return mode;
        }
    }

    public void changeValue(float delta)
    {
        value += delta;
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
        durationRemaining = buff.durration;
        timeScale = buff.scales.time;
        castCount = buff.castCount;
        slot = buff.slot;
        if (castCount > 0)
        {
            mode = BuffMode.Cast;
        }
    }

    public void setupShield(Scales scales, float durationBegin, float valueBegin, float regenBegin = 0)
    {
        durationMax = durationBegin;
        durationRemaining = durationBegin;
        timeScale = scales.time;
        mode = BuffMode.Shield;
        valueMax = valueBegin;
        value = valueBegin;
        regen = regenBegin;
    }

    public void setupDot(Scales scales, float durationBegin, HarmValues harmV)
    {
        durationMax = durationBegin;
        durationRemaining = durationBegin;
        timeScale = scales.time;
        mode = BuffMode.Dot;
        harm = harmV;
    }

    // Update is called once per frame
    void Tick()
    {
        if (isServer)
        {
            switch (mode)
            {
                case BuffMode.Dot:
                    float timeThisTick = Mathf.Min(durationRemaining, Time.fixedDeltaTime);
                    HarmValues harmThisTick = harm.tickPortion( timeThisTick / durationMax);
                    events.fireHit(new GetHitEventData()
                    {
                        harm = harmThisTick,
                    });
                    break;
                case BuffMode.Shield:
                    value += regen * Time.fixedDeltaTime;
                    value = Mathf.Min(value, valueMax);
                    break;
            }
        }
        durationRemaining -= Time.fixedDeltaTime;
        if (isServer)
        {
            if (durationRemaining <= 0 || (mode == BuffMode.Cast && castCount <= 0))
            {
                Destroy(gameObject);
            }


        }
    }
}
