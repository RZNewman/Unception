using Mirror;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static EventManager;
using static GenerateBuff;

public class Health : NetworkBehaviour, BarValue
{
    [SyncVar]
    float maxHealth;

    [SyncVar]
    float currentHealth;

    Combat combat;
    GameObject damageDisplayPre;
    GlobalPlayer gp;

    Dictionary<BuffMode, List<Buff>> buffReferences = new Dictionary<BuffMode, List<Buff>>();
    // Start is called before the first frame update
    void Start()
    {

        combat = GetComponent<Combat>();
        damageDisplayPre = FindObjectOfType<GlobalPrefab>().DamageNumberPre;
        gp = FindObjectOfType<GlobalPlayer>();
        GetComponent<EventManager>().HitEvent += OnGetHit;
        buffReferences[BuffMode.Dot] = new List<Buff>();
        buffReferences[BuffMode.Expose] = new List<Buff>();
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
    public void takeDamageHit(float damage)
    {
        takeDamageShielded(damage);
        RpcDisplayDamage(damage);
    }

    [Server]
    public void takeDamageDrain(float damage)
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
        if (!hasAuthority)
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
                clearReferences(BuffMode.Dot);
                clearReferences(BuffMode.Expose);
            }
            if (currentHealth <= 0)
            {
                GetComponent<LifeManager>().die();

            }
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

    void clearReferences(BuffMode mode)
    {
        buffReferences[mode].ForEach(b =>
        {
            Destroy(b.gameObject);
        });
    }

    void OnGetHit(GetHitEventData data)
    {
        if (isServer)
        {
            buffReferences[BuffMode.Expose].Sort((a, b) => a.remainingDuration.CompareTo(b.remainingDuration));
            float incDamage = data.damage;

            float addedDamage = 0;
            for (int i = 0; i < buffReferences[BuffMode.Expose].Count; i++)
            {
                Buff ex = buffReferences[BuffMode.Expose][i];
                float exposeValue = Mathf.Min(incDamage, ex.valueCurrent);
                ex.changeValue(-exposeValue);
                addedDamage += exposeValue;
                incDamage -= exposeValue;

                if (incDamage <= 0)
                {
                    break;
                }
            }
            takeDamageDrain(addedDamage);
        }
    }


    public BarValue.BarData getBarFill()
    {
        float riskDot = buffReferences[BuffMode.Dot].Sum(b => b.valueByTime);
        riskDot = Mathf.Min(riskDot, currentHealth);
        float riskExpose = buffReferences[BuffMode.Expose].Sum(b => b.valueCurrent);
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
