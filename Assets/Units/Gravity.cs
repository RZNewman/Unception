using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : NetworkBehaviour 
{
    // Start is called before the first frame
    public float gravity = -9.81f;
    UnitMovement movement;
    Rigidbody rb;
    LifeManager lifeManager;
    void Start()
    {
        movement = GetComponent<UnitMovement>();
        rb = GetComponent<Rigidbody>();
        lifeManager = GetComponent<LifeManager>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isServer && !movement.grounded && !lifeManager.IsDead)
		{
            rb.velocity += new Vector3(0, gravity, 0) *Time.fixedDeltaTime;
		}
    }
}
