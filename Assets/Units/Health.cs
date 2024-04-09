using Mirror;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using static EventManager;
using static GenerateBuff;
using static AttackUtils;
using static GenerateHit;

public class Health : NetworkBehaviour, BarValue
{
    [SyncVar]
    float maxHealth;

    [SyncVar]
    float currentHealth;

    [SyncVar]
    float exposedHealth;

    Combat combat;
    GameObject damageDisplayPre;
    GlobalPlayer gp;
    Power power;
    UnitMovement mover;

    Dictionary<BuffMode, List<Buff>> buffReferences = new Dictionary<BuffMode, List<Buff>>();
    // Start is called before the first frame update
    void Start()
    {

        combat = GetComponent<Combat>();
        damageDisplayPre = FindObjectOfType<GlobalPrefab>().DamageNumberPre;
        gp = FindObjectOfType<GlobalPlayer>();
        mover = GetComponent<UnitMovement>();
        power = GetComponent<Power>();

        GetComponent<EventManager>().HitEvent += OnGetHit;
        GetComponent<EventManager>().ApplyEvent += OnApply;
        buffReferences[BuffMode.Dot] = new List<Buff>();
        buffReferences[BuffMode.Shield] = new List<Buff>();

        if (isServer)
        {
            maxHealth = 1;
            currentHealth = maxHealth;
            GetComponent<Power>().subscribePower(updateMaxHealth);
        }

    }

    void updateMaxHealth(Power p)
    {
        float currentPercent = currentHealth / maxHealth;
        UnitPropsHolder holder = GetComponent<UnitPropsHolder>();
        maxHealth = holder.props.maxHealthMult * holder.championHealthMultiplier * p.power;
        currentHealth = maxHealth * currentPercent;
    }

    [Server]
    void takeDamageHit(float damage)
    {
        takeDamageShielded(damage);
        RpcDisplayDamage(damage);
    }

    [Server]
    public void takeDamageNoDisplay(float damage)
    {
        takeDamageShielded(damage);
    }

    void takeDamageShielded(float damage)
    {
        buffReferences[BuffMode.Shield].Sort((a, b) => a.remainingDuration.CompareTo(b.remainingDuration));

        for (int i = 0; i < buffReferences[BuffMode.Shield].Count; i++)
        {
            Buff shield = buffReferences[BuffMode.Shield][i];
            float shieldValue = Mathf.Min(damage, shield.valueCurrent);
            shield.changeValue(-shieldValue);
            damage -= shieldValue;
            if (damage <= 0)
            {
                break;
            }
        }
        currentHealth -= damage;
    }

    public void takePercentDamage(float percent)
    {
        currentHealth -= maxHealth * percent;
    }

    public void healPercent(float percent)
    {
        currentHealth += maxHealth * percent;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    [ClientRpc]
    void RpcDisplayDamage(float damage)
    {
        //Not the local player
        if (!isOwned)
        {
            Vector3 offset = Random.insideUnitSphere * 4;
            GameObject o = Instantiate(damageDisplayPre, transform.position + offset, Quaternion.identity);
            o.transform.localScale *= 4 * (damage / (gp.player.power * 0.4f)) * 0.9f;
            o.GetComponentInChildren<TMP_Text>().text = Power.displayPower(damage);
        }

    }

    // Update is called once per frame
    public void OrderedUpdate()
    {
        if (!isServer)
        {
            return;
        }
        if (!GetComponent<LifeManager>().IsDead)
        {
            if (!combat.inCombat)
            {
                //healed in Pack heal now
                //currentHealth = maxHealth;
                currentHealth = Mathf.Max(currentHealth, 1);
                exposedHealth = 0;
                clearOOCDebuffs(BuffMode.Dot);
            }
            if (currentHealth <= 0)
            {
                GetComponent<LifeManager>().die();

            }
            //if(exposedHealth > 0)
            //{
            //    exposedHealth -= maxHealth * 0.005f * Time.fixedDeltaTime * power.scaleTime();
            //    exposedHealth = Mathf.Max(exposedHealth, 0);
            //}
        }



    }


    public void addReference(Buff b)
    {
        buffReferences[b.buffMode].Add(b);
    }
    public void removeReference(Buff b)
    {
        buffReferences[b.buffMode].Remove(b);
    }

    void clearOOCDebuffs(BuffMode mode)
    {
        buffReferences[mode].ForEach(b =>
        {
            b.clearOOC();
        });
    }

    void OnGetHit(GetHitEventData data)
    {
        if (isServer)
        {
            float damageMult = 1;
            if(mover && mover.isIncapacitated) { damageMult = 1.1f; }

            data.harm.multDamage(damageMult);
            float incDamage = data.harm.damage;

            if (!data.stopExpose)
            {
                float addedDamage = Mathf.Min(incDamage, exposedHealth);
                exposedHealth -= addedDamage;
                takeDamageNoDisplay(addedDamage);
            }
            




            float exposeDamage = 0;
            if (data.harm.exposePercent > 0)
            {
                exposeDamage = incDamage * data.harm.exposePercent;
                incDamage -= exposeDamage;
                exposeDamage *= EXPOSE_MULTIPLIER;
            }
            exposedHealth += exposeDamage;
            Debug.Log(data.harm.damage + " - " + incDamage + " - " + exposeDamage);
            takeDamageHit(incDamage);
        }
    }
    void OnApply(ApplyDotEventData data)
    {
        if (isServer)
        {

            SpawnDot(transform, data.harm.scales, data.time, data.harm);

        }
    }


    public BarValue.BarData getBarFill()
    {
        float riskDot = buffReferences[BuffMode.Dot].Sum(b => b.damageRemaining);
        riskDot = Mathf.Min(riskDot, currentHealth);
        float riskExpose = exposedHealth;
        riskExpose = Mathf.Min(riskExpose, currentHealth - riskDot);
        float shield = buffReferences[BuffMode.Shield].Sum(b => b.valueCurrent);


        float riskHealth = riskDot + riskExpose;
        float safeHealth = Mathf.Max(currentHealth - riskHealth, 0);

        float barMax = Mathf.Max(maxHealth, currentHealth + shield);
        return new BarValue.BarData
        {

            active = true,
            segments = new UiBarBasic.BarSegment[]
            {
                new UiBarBasic.BarSegment
                {
                    color = combat.inCombat ? Color.red : new Color(1, 0.5f, 0),
                    percent = Mathf.Clamp01(safeHealth / barMax),
                },
                new UiBarBasic.BarSegment
                {
                    color = new Color(1f, 0.5f, 0),
                    percent = Mathf.Clamp01(riskExpose / barMax),
                },
                new UiBarBasic.BarSegment
                {
                    color = new Color(0.7f, 0, 0),
                    percent = Mathf.Clamp01(riskDot / barMax),
                },
                new UiBarBasic.BarSegment
                {
                    color = new Color(1f, 1, 1),
                    percent = Mathf.Clamp01(shield / barMax),
                },
            }
        };
    }

}
