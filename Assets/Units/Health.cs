using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SyncVar]
    public float maxHealth;

    float currenthealth;
    // Start is called before the first frame update
    void Start()
    {
        currenthealth = maxHealth;
    }

    public void takeDamage(float damage)
    {
        currenthealth -= damage;
    }

    // Update is called once per frame
    void Update()
    {
        if(currenthealth <= 0)
        {
            GetComponent<LifeManager>().die();

        }
    }
    public float percent
    {
        get
        {
            return Mathf.Clamp01(currenthealth / maxHealth);
        }
    }
}
