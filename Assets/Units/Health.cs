using Mirror;
using TMPro;
using UnityEngine;

public class Health : NetworkBehaviour, BarValue
{
    [SyncVar]
    float maxHealth;

    [SyncVar]
    float currentHealth;

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
    public void takeDamage(float damage)
    {
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
