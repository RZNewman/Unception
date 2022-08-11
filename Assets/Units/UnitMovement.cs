using Mirror;
using System;
using UnityEngine;
using static DashState;
using static GenerateDash;
using static UnitControl;
using static Utils;
public class UnitMovement : NetworkBehaviour
{
    UnitPropsHolder propHolder;
    Rigidbody rb;
    ControlManager controller;
    LifeManager lifeManager;
    StateMachine<PlayerMovementState> movement;
    ModelLoader model;
    Power power;

    [HideInInspector]
    public float currentLookAngle = 0;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        power = GetComponent<Power>();
        controller = GetComponent<ControlManager>();
        model = GetComponent<ModelLoader>();
        movement = new StateMachine<PlayerMovementState>(() => new FreeState(this));
        lifeManager = GetComponent<LifeManager>();
        propHolder = GetComponent<UnitPropsHolder>();
        lifeManager.suscribeDeath(cleanup);
    }

    public UnitProperties props
    {
        get
        {
            return propHolder.props;
        }
    }

    public GameObject getSpawnBody()
    {
        return GetComponentInChildren<UnitRotation>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (isClientOnly && model.modelLoaded)
        {
            setGround();
        }
    }
    public void ServerUpdate()
    {
        if (!model.modelLoaded)
        {
            return;
        }
        else if (!lifeManager.IsDead)
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
        if (!lifeManager.IsDead && model.modelLoaded)
        {
            setGround();
            movement.transition();
        }

    }
    void cleanup()
    {
        movement.exit();
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
        rb.velocity = new Vector3(rb.velocity.x, props.jumpForce * power.scale(), rb.velocity.z);
    }
    public void applyForce(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
    }

    public void move(UnitInput inp, float speedMultiplier = 1.0f, float accMultiplier = 1.0f)
    {
        float lookMultiplier = toMoveMultiplier(inp.move);
        float airMultiplier = 1.0f;



        if (!grounded)
        {
            airMultiplier = 0.6f;
        }
        Vector3 desiredDirection = input2vec(inp.move);

        speedMultiplier *= lookMultiplier * airMultiplier;


        float potentialSpeed = props.maxSpeed * speedMultiplier * power.scale();
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
        float stoppingFrameMag = props.decceleration * stoppingMult * Time.fixedDeltaTime * power.scale();

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
        float addingFrameMag = props.acceleration * addingMult * Time.fixedDeltaTime * power.scale();

        if (diff.magnitude <= addingFrameMag)
        {
            planarVelocity = desiredVeloicity;

        }
        else
        {
            planarVelocity += diff.normalized * addingFrameMag;
        }

    }

    public DashInstanceData baseDash()
    {
        return new DashInstanceData
        {
            distance = props.dashDistance * power.scale(),
            speed = props.dashSpeed * power.scale(),
            control = DashControl.Input,
            endMomentum = DashEndMomentum.Walk,
        };
    }
    public void dash(UnitInput inp, float dashSpeed, DashControl control)
    {
        Vector3 desiredDirection;
        switch (control)
        {
            case DashControl.Forward:
                desiredDirection = getSpawnBody().transform.forward;
                break;
            case DashControl.Backward:
                desiredDirection = -getSpawnBody().transform.forward;
                break;
            case DashControl.Input:
                desiredDirection = input2vec(inp.move);
                break;
            default:
                desiredDirection = Vector3.zero;
                break;
        }
        planarVelocity = desiredDirection * dashSpeed;

    }
    public void setToWalkSpeed()
    {
        float lookMultiplier = toMoveMultiplier(vec2input(planarVelocity));
        float airMultiplier = 1.0f;
        if (!grounded)
        {
            airMultiplier = 0.6f;
        };

        float potentialSpeed = props.maxSpeed * lookMultiplier * airMultiplier * power.scale();

        planarVelocity = planarVelocity.normalized * potentialSpeed;
    }

    float toMoveMultiplier(Vector2 inputMove)
    {
        if (inputMove == Vector2.zero)
        {
            return 1f;
        }
        float inputAngle = -Vector2.SignedAngle(Vector2.up, inputMove);
        float angleDiff = Mathf.Abs(normalizeAngle(inputAngle - currentLookAngle));


        if (angleDiff > 90)
        {
            return Mathf.Lerp(props.sidewaysMoveMultiplier, props.backwardsMoveMultiplier, (angleDiff - 90) / 90);
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

        bool terrain = Physics.SphereCast(transform.position, model.size.scaledRadius, -transform.up, out rout, model.size.scaledHalfHeight * 1.01f, LayerMask.GetMask("Terrain"));
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
                return mag <= 0.05f * power.scale();
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
