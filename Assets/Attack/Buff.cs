using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EventManager;
using static GenerateAttack;
using static GenerateBuff;
using static GenerateHit.HitInstanceData;
using static StatTypes;

public class Buff : NetworkBehaviour, Duration
{
    [SyncVar]
    float _durationRemaining;

    [SyncVar]
    float _durationMax;

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

    BuffManager manager;

    float timeScale;
    public float relativeScale(float targetTimeScale)
    {
        return timeScale / targetTimeScale;
    }

    public float remainingDuration
    {
        get
        {
            return _durationRemaining;
        }
    }
    public float maxDuration
    {
        get
        {
            return _durationMax;
        }
    }

    Duration controllingDuration;
    
    public Duration duration {
       get { return controllingDuration; } 
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
        if (selfManaged)
        {
            transform.parent.GetComponent<BuffManager>().addBuff(this);
        }
        
    }


    private void OnDestroy()
    {
        if (selfManaged)
        {
            transform.parent.GetComponent<BuffManager>().removeBuff(this);
        }
        
    }

    public float damageRemaining
    {
        get
        {
            //Debug.Log(harm.totalDamage + " - " + progressPercent);
            return harm.totalDamage * controllingDuration.remainingPercent;
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

     public void OnCast(Ability a)
    {
        ItemSlot? castSlot = a.slot();
        if (mode == BuffMode.Cast && castSlot.HasValue)
        {
            if (!slot.HasValue || slot.Value == castSlot.Value)
            {
                castCount--;
            }

        }

    }

    public void setup(BuffInstanceData buff)
    {
        _durationMax = buff.durration;
        _durationRemaining = buff.durration;
        controllingDuration = this;
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
        _durationMax = durationBegin;
        _durationRemaining = durationBegin;
        controllingDuration = this;
        timeScale = scales.time;
        mode = BuffMode.Shield;
        valueMax = valueBegin;
        value = valueBegin;
        regen = regenBegin;
    }

    public void setupDot(Scales scales, float durationBegin, HarmValues harmV)
    {
        _durationMax = durationBegin;
        _durationRemaining = durationBegin;
        controllingDuration = this;
        timeScale = scales.time;
        mode = BuffMode.Dot;
        harm = harmV;
    }

    public void setupAura(Scales scales, Persistent aura, HarmValues harmV)
    {
        controllingDuration = aura;
        timeScale = scales.time;
        mode = BuffMode.Dot;
        harm = harmV;
    }

    // Update is called once per frame
     public void Tick()
    {
        if (isServer)
        {
            switch (mode)
            {
                case BuffMode.Dot:
                    float timeThisTick = Mathf.Min(controllingDuration.remainingDuration, Time.fixedDeltaTime);
                    HarmValues harmThisTick = harm.tickPortion( timeThisTick / controllingDuration.maxDuration);
                    Debug.Log(harmThisTick.damage);
                    manager.eventManager.fireHit(new GetHitEventData()
                    {
                        harm = harmThisTick,
                        stopExpose = true,
                    });
                    break;
                case BuffMode.Shield:
                    value += regen * Time.fixedDeltaTime;
                    value = Mathf.Min(value, valueMax);
                    break;
            }
        }
        if(selfManaged)
        {
            _durationRemaining -= Time.fixedDeltaTime;
            if (isServer)
            {
                if (_durationRemaining <= 0 || (mode == BuffMode.Cast && castCount <= 0))
                {
                    Destroy(gameObject);
                }


            }
        }
        
    }

    public void clearOOC()
    {
        if (selfManaged)
        {
            Destroy(gameObject);
        }
    }

    internal void setManager(BuffManager buffManager)
    {
        manager = buffManager;
    }

    bool selfManaged
    {
        get
        {
            return (Object)controllingDuration == this;
        }
    }
}
