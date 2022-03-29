using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;
public class UnitMovement : NetworkBehaviour
{
    public float maxSpeed = 5f;
    public float acceleration = 5f;
    public float jumpForce = 10f;
    public float jumpsquatTime = 0.8f;
    Rigidbody rb;
    UnitControl controller;
    StateMachine<PlayerMovementState> movement;
    CapsuleCollider col;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<UnitControl>();
        col = GetComponentInChildren<CapsuleCollider>();
        movement = new StateMachine<PlayerMovementState>(new FreeState(this));
        
    }

    // Update is called once per frame
    void Update()
    {
		
    }
	private void FixedUpdate()
	{
        if (isServer)
        {
            setGround();
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

    public void move(Vector3 desiredDirection, float speedMultiplier, float accMultiplier)
	{
        Vector3 desiredSpeed = desiredDirection * maxSpeed * speedMultiplier;
        float frameMagnitude = acceleration * accMultiplier * Time.fixedDeltaTime;
        Vector3 diff = desiredSpeed - planarVelocity;
		if (diff.magnitude <= frameMagnitude)
		{
            planarVelocity = desiredSpeed;

        }
		else
		{
            planarVelocity += diff.normalized * frameMagnitude;
		}

    }

    bool ground = false;
    Vector3 groundNormal;
    void setGround()
    {

        RaycastHit rout;

        bool terrain = Physics.SphereCast(transform.position, col.radius, -transform.up, out rout, 1.01f, LayerMask.GetMask("Terrain"));
        float angle = Vector3.Angle(Vector3.up, rout.normal);

        ground = terrain && angle < 45;

        if (ground)
        {
            groundNormal = rout.normal;
        }
        else
        {
            groundNormal = Vector3.up;
        }


    }
    public bool grounded
    {
        get
        {

            if (ground)
            {
                float mag = Vector3.Dot(rb.velocity, groundNormal);
                return mag <= 0.05f;
            }
            return false;

        }
    }

}
