using Mirror;
using System.Collections;
using System.Collections.Generic;
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
        maxHealth = GetComponent<UnitPropsHolder>().props.maxHealth;
        currentHealth = maxHealth;
        combat = GetComponent<Combat>();
    }

    public void takeDamage(float damage)
    {
        currentHealth -= damage;
    }

    // Update is called once per frame
    public void ServerUpdate()
    {
        if (!combat.inCombat)
        {
            currentHealth = maxHealth;
        }
        if(currentHealth <= 0)
        {
            GetComponent<LifeManager>().die();

        }
    }

    public BarValue.BarData getBarFill()
    {
        return new BarValue.BarData
        {
            color = combat.inCombat ? Color.red: new Color(1,0.5f,0),
            fillPercent = Mathf.Clamp01(currentHealth / maxHealth),
            active = true,
        };
    }

}
