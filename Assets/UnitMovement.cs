using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;
using static Utils;
public class UnitMovement : NetworkBehaviour
{
    public float maxSpeed = 5f;
    public float acceleration = 5f;
    public float jumpForce = 10f;
    public float jumpsquatTime = 0.8f;
    public float lookSpeedDegrees = 270f;
    public float sidewaysMoveMultiplier = 0.85f;
    public float backwardsMoveMultiplier = 0.7f;
    Rigidbody rb;
    ControlManager controller;
    StateMachine<PlayerMovementState> movement;
    CapsuleCollider col;

    [HideInInspector]
    public float currentLookAngle=0;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<ControlManager>();
        controller.spawnControl();
        col = GetComponentInChildren<CapsuleCollider>();
        movement = new StateMachine<PlayerMovementState>(() => new FreeState(this));
        
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

	public UnitInput input
	{
		get
		{
            return controller.GetUnitInput();
		}
	}
    public Vector3 planarVelocity
	{
		get
		{
            Vector3 plane3 = Vector3.ProjectOnPlane(rb.velocity, groundNormal);
            Quaternion rot = Quaternion.AngleAxis(-Vector3.Angle(groundNormal, Vector3.up), Vector3.Cross(Vector3.up, groundNormal));
            return rot * plane3;
        }
		set
		{
            Vector3 plane3 = Vector3.ProjectOnPlane(rb.velocity, groundNormal);
            Vector3 vertdiff = rb.velocity - plane3;

            Vector3 move3 = new Vector3(value.x, 0, value.z);
            Quaternion rot = Quaternion.AngleAxis(Vector3.Angle(groundNormal, Vector3.up), Vector3.Cross(Vector3.up, groundNormal));
            Vector3 plane3New = rot * move3;
            rb.velocity = plane3New + vertdiff;
        }
	}

    public void jump()
	{
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.y);
	}

    public void move(Vector3 desiredDirection, float speedMultiplier, float accMultiplier)
	{
        float potentialSpeed = maxSpeed * speedMultiplier;
        float desiredSpeed;
		if (grounded)
		{
            desiredSpeed = potentialSpeed;

        }
        else
		{
            float usefulSpeed = Mathf.Max(Vector3.Dot(planarVelocity, desiredDirection), 0);
            desiredSpeed = Mathf.Max(usefulSpeed, potentialSpeed);
        }
        Vector3 desiredVeloicity = desiredDirection * desiredSpeed;
        float frameMagnitude = acceleration * accMultiplier * Time.fixedDeltaTime;
        Vector3 diff = desiredVeloicity - planarVelocity;
		if (diff.magnitude <= frameMagnitude)
		{
            planarVelocity = desiredVeloicity;

        }
		else
		{
            planarVelocity += diff.normalized * frameMagnitude;
		}

    }
    public void rotate(float desiredAngle, float speedMultiplier)
	{
        float diff = desiredAngle - currentLookAngle;
        float frameMagnitude = lookSpeedDegrees * speedMultiplier * Time.fixedDeltaTime;
        diff = normalizeAngle(diff);
        if (Mathf.Abs(diff) <= frameMagnitude)
		{
            currentLookAngle = desiredAngle;
		}
		else
		{
            currentLookAngle += frameMagnitude * Mathf.Sign(diff);
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
