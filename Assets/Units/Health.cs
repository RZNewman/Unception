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
    // Start is called before the first frame update
    void Start()
    {
        maxHealth = GetComponent<UnitPropsHolder>().props.maxHealth;
        currentHealth = maxHealth;
    }

    public void takeDamage(float damage)
    {
        currentHealth -= damage;
    }

    // Update is called once per frame
    public void ServerUpdate()
    {
        if(currentHealth <= 0)
        {
            GetComponent<LifeManager>().die();

        }
    }

    public BarValue.BarData getBarFill()
    {
        return new BarValue.BarData
        {
            color = Color.red,
            fillPercent = Mathf.Clamp01(currentHealth / maxHealth),
            active = true,
        };
    }

}
