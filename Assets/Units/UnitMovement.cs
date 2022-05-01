using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;
using static Utils;
public class UnitMovement : NetworkBehaviour
{
    UnitPropsHolder propHolder;
    Rigidbody rb;
    ControlManager controller;
    LifeManager lifeManager;
    StateMachine<PlayerMovementState> movement;
    CapsuleCollider col;

    [HideInInspector]
    public float currentLookAngle=0;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<ControlManager>();
        col = GetComponentInChildren<CapsuleCollider>();
        movement = new StateMachine<PlayerMovementState>(() => new FreeState(this));
        lifeManager = GetComponent<LifeManager>();
        propHolder = GetComponent<UnitPropsHolder>();
    }

    public UnitProperties props
    {
        get
        {
            return propHolder.props;
        }
    }

    // Update is called once per frame
    void Update()
    {
		
    }
	public void ServerUpdate()
	{
        if (!lifeManager.IsDead)
        {           
            movement.tick();
        }
        else
        {
            planarVelocity = Vector3.zero;
        }
    }
    public void ServerTransition()
    {
        if (!lifeManager.IsDead)
        {
            setGround();
            movement.transition();
        }
        
    }
    public Posture posture
    {
        get { return GetComponent<Posture>(); }
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
        rb.velocity = new Vector3(rb.velocity.x, props.jumpForce, rb.velocity.z);
	}
    public void applyForce(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
    }

    public void move(UnitInput inp, float speedMultiplier=1.0f, float accMultiplier =1.0f)
	{
        float lookMultiplier = toMoveMultiplier(inp.move);
        float airMultiplier=1.0f;

        

        if (!grounded)
        {
            airMultiplier = 0.6f;
        }
        Vector3 desiredDirection = input2vec(inp.move);

        speedMultiplier *= lookMultiplier* airMultiplier;
        

        float potentialSpeed = props.maxSpeed * speedMultiplier;
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
        Vector3 diff = desiredVeloicity - planarVelocity;
        float stoppingMagnitude = Vector3.Dot(diff, -planarVelocity);
        stoppingMagnitude = Mathf.Max(stoppingMagnitude, 0);
        Vector3 stoppingDir = -planarVelocity.normalized * stoppingMagnitude;
        float stoppingMult = accMultiplier * airMultiplier;
        float stoppingFrameMag = props.decceleration *stoppingMult * Time.fixedDeltaTime;
        
        if (stoppingDir.magnitude <= stoppingFrameMag)
		{
            planarVelocity += stoppingDir;

        }
		else
		{
            planarVelocity += stoppingDir.normalized * stoppingFrameMag;
		}

        diff = desiredVeloicity - planarVelocity;
        float lookMultiplierDiff = toMoveMultiplier(vec2input(diff));
        float addingMult = accMultiplier * airMultiplier * lookMultiplierDiff;
        float addingFrameMag = props.acceleration * addingMult * Time.fixedDeltaTime;

        if (diff.magnitude <= addingFrameMag)
        {
            planarVelocity =desiredVeloicity;

        }
        else
        {
            planarVelocity += diff.normalized * addingFrameMag;
        }

    }

    float toMoveMultiplier(Vector2 inputMove)
    {
        if(inputMove == Vector2.zero)
        {
            return 1f;
        }
        float inputAngle = -Vector2.SignedAngle(Vector2.up, inputMove);
        float angleDiff = Mathf.Abs(normalizeAngle(inputAngle - currentLookAngle));


        if (angleDiff > 90)
        {
            return  Mathf.Lerp(props.sidewaysMoveMultiplier, props.backwardsMoveMultiplier, (angleDiff - 90) / 90);
        }
        else
        {
            return Mathf.Lerp(1.0f, props.sidewaysMoveMultiplier, angleDiff / 90);
        }
    }
    public void rotate(UnitInput inp, float speedMultiplier = 1.0f)
	{
        if (inp.look == Vector2.zero)
        {
            return;
        }
        float desiredAngle = -Vector2.SignedAngle(Vector2.up, inp.look);
        float diff = desiredAngle - currentLookAngle;
        float frameMagnitude = props.lookSpeedDegrees * speedMultiplier * Time.fixedDeltaTime;
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

    public Vector3 floorNormal
    {
        get
        {
            return groundNormal;
        }
    }


}
