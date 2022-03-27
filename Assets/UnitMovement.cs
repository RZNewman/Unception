using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;
public class UnitMovement : NetworkBehaviour
{
    public float baseSpeed = 5f;
    public float jumpForce = 10f;
    Rigidbody rb;
    UnitControl controller;
    StateMachine<PlayerMovementState> movement;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        controller = GetComponentInParent<UnitControl>();
        movement = new StateMachine<PlayerMovementState>(new FreeState(this));
    }

    // Update is called once per frame
    void Update()
    {
		if (isServer)
		{
            movement.tick();
		}
    }

    public UnitControl control
	{
		get
		{
            return controller;
		}
	}
    public Vector3 planarVelocity
	{
		get
		{
            return new Vector3(rb.velocity.x, 0, rb.velocity.z);
		}
		set
		{
            rb.velocity = new Vector3(value.x, rb.velocity.y, value.z);
		}
	}

    public void jump()
	{
        rb.velocity += new Vector3(0, jumpForce, 0);
	}

}
