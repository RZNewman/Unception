using Mirror;
using UnityEngine;

public class Health : NetworkBehaviour, BarValue
{
    [SyncVar]
    float maxHealth;

    [SyncVar]
    float currentHealth;

    Combat combat;
    // Start is called before the first frame update
    void Start()
    {

        combat = GetComponent<Combat>();
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

    public void takeDamage(float damage)
    {
        currentHealth -= damage;
    }
    public void takePercentDamage(float percent)
    {
        currentHealth -= maxHealth * percent;
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
            }
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

    public BarValue.BarData getBarFill()
    {
        return new BarValue.BarData
        {
            color = combat.inCombat ? Color.red : new Color(1, 0.5f, 0),
            fillPercent = Mathf.Clamp01(currentHealth / maxHealth),
            active = true,
        };
    }

}
