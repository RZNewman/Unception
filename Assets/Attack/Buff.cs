using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EventManager;
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

    [SyncVar]
    float value;

    [SyncVar]
    float valueMax;

    [SyncVar]
    float regen;

    BuffMode mode = BuffMode.Timed;

    float timeScale;
    EventManager events;
    Health health;
    public float relativeScale(float targetTimeScale)
    {
        return timeScale / targetTimeScale;
    }

    public float remainingDuration
    {
        get
        {
            return duration;
        }
    }

    public float progressPercent
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
        health = GetComponentInParent<Health>();
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

    public float valueRemaining
    {
        get
        {
            return value * (1-progressPercent);
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
        duration = buff.durration;
        timeScale = buff.scales.time;
        castCount = buff.castCount;
        slot = buff.slot;
        if (castCount > 0)
        {
            mode = BuffMode.Cast;
        }
    }

    public void setup(BuffMode buffMode, Scales scales, float durationBegin, float valueBegin, float regenBegin = 0)
    {
        durationMax = durationBegin;
        duration = durationBegin;
        timeScale = scales.time;
        mode = buffMode;
        valueMax = valueBegin;
        value = valueBegin;
        regen = regenBegin;
    }

    // Update is called once per frame
    void Tick()
    {
        if (isServer)
        {
            switch (mode)
            {
                case BuffMode.Dot:
                    float timeThisTick = Mathf.Min(duration, Time.fixedDeltaTime);
                    float damageThisTick = timeThisTick * value/durationMax;
                    health.takeDamageDrain(damageThisTick);
                    break;
                case BuffMode.Shield:
                    value += regen * Time.fixedDeltaTime;
                    value = Mathf.Min(value, valueMax);
                    break;
            }
        }
        duration -= Time.fixedDeltaTime;
        if (isServer)
        {
            if (duration <= 0 || (mode == BuffMode.Cast && castCount <= 0))
            {
                Destroy(gameObject);
            }
            if (mode == BuffMode.Expose && value <= 0)
            {
                Destroy(gameObject);
            }


        }
    }
}
