using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Health : NetworkBehaviour, BarValue
{
    [SyncVar]
    float maxHealth;

    [SyncVar]
    float currentHealth;

    [SyncVar]
    float riskDot;

    [SyncVar]
    float riskExpose;

    Combat combat;
    GameObject damageDisplayPre;
    GlobalPlayer gp;
    // Start is called before the first frame update
    void Start()
    {

        combat = GetComponent<Combat>();
        damageDisplayPre = FindObjectOfType<GlobalPrefab>().DamageNumberPre;
        gp = FindObjectOfType<GlobalPlayer>();
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
    public void takeDamage(float damage, float strength)
    {
        //TODO scale strength with power of attacker
        damage += hitExposes(strength);
        currentHealth -= damage;
        RpcDisplayDamage(damage);
    }
    public void takePercentDamage(float percent)
    {
        currentHealth -= maxHealth * percent;
    }

    [ClientRpc]
    void RpcDisplayDamage(float damage)
    {
        Vector3 offset = Random.insideUnitSphere * 4;
        GameObject o = Instantiate(damageDisplayPre, transform.position + offset, Quaternion.identity);
        o.transform.localScale *= 4 * (damage / (gp.player.power * 0.4f)) * 0.9f;
        o.GetComponentInChildren<TMP_Text>().text = Power.displayPower(damage);
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
                currentHealth = maxHealth;
                dots.Clear();
            }
            tickDots();
            tickExposes();
            if (currentHealth <= 0)
            {
                GetComponent<LifeManager>().die();

            }
        }



    }

    public void healToFull()
    {
        currentHealth = maxHealth;
    }

    struct Dot
    {
        public float time;
        public float damage;
    }

    List<Dot> dots = new List<Dot>();
    public void addDot(float duraton, float damage)
    {
        dots.Add(new Dot() { time = duraton, damage = damage });
    }

    void tickDots()
    {
        float newRisk = 0;
        for (int i = 0; i < dots.Count; i++)
        {
            Dot dot = dots[i];
            bool finalTick = dot.time <= Time.fixedDeltaTime;
            float dotTime = finalTick ? dot.time : Time.fixedDeltaTime;


            float damage = dotTime / dot.time * dot.damage;
            dot.time -= dotTime;
            dot.damage -= damage;
            currentHealth -= damage;
            dots[i] = dot;
            newRisk += dot.damage;
        }
        int k = 0;
        while (k < dots.Count)
        {
            if (dots[k].time <= 0)
            {
                dots.RemoveAt(k);
            }
            else
            {
                k++;
            }
        }
        riskDot = newRisk;
    }

    struct Expose
    {
        public float time;
        public float damage;
        public float remainingStrength;
    }

    List<Expose> exposes = new List<Expose>();
    public void addExpose(float duraton, float damage, float strength)
    {
        exposes.Add(new Expose() { time = duraton, damage = damage, remainingStrength = strength });
    }

    void tickExposes()
    {
        float newRisk = 0;
        for (int i = 0; i < exposes.Count; i++)
        {
            Expose ex = exposes[i];
            ex.time -= Time.fixedDeltaTime;
            exposes[i] = ex;
            newRisk += ex.damage;
        }
        int k = 0;
        while (k < exposes.Count)
        {
            if (exposes[k].time <= 0 || exposes[k].remainingStrength <= 0)
            {
                exposes.RemoveAt(k);
            }
            else
            {
                k++;
            }
        }
        riskExpose = newRisk;
    }
    float hitExposes(float strength)
    {
        float addedDamage = 0;
        for (int i = 0; i < exposes.Count; i++)
        {
            Expose ex = exposes[i];
            float damage = Mathf.Clamp01(strength / ex.remainingStrength) * ex.damage;
            ex.remainingStrength -= strength;
            ex.damage -= damage;
            exposes[i] = ex;
            addedDamage += damage;
        }
        return addedDamage;
    }


    public BarValue.BarData getBarFill()
    {
        float riskHealth = riskDot + riskExpose;
        float safeHealth = Mathf.Max(currentHealth - riskHealth, 0);
        return new BarValue.BarData
        {

            active = true,
            segments = new UiBarBasic.BarSegment[]
            {
                new UiBarBasic.BarSegment
                {
                    color = combat.inCombat ? Color.red : new Color(1, 0.5f, 0),
                    percent = Mathf.Clamp01(safeHealth / maxHealth),
                },
                new UiBarBasic.BarSegment
                {
                    color = new Color(1f, 0.5f, 0),
                    percent = Mathf.Clamp01(riskExpose / maxHealth),
                },
                new UiBarBasic.BarSegment
                {
                    color = new Color(0.7f, 0, 0),
                    percent = Mathf.Clamp01(riskDot / maxHealth),
                },
            }
        };
    }

}
