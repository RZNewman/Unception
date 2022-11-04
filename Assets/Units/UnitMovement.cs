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
    Size size;
    Power power;
    FloorNormal ground;

    public float syncAngleHard = 40;
    [HideInInspector]
    [SyncVar(hook = nameof(syncLookAngle))]
    public float currentLookAngle = 0;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        power = GetComponent<Power>();
        controller = GetComponent<ControlManager>();
        movement = new StateMachine<PlayerMovementState>(() => new FreeState(this));
        lifeManager = GetComponent<LifeManager>();
        propHolder = GetComponent<UnitPropsHolder>();
        ground = GetComponent<FloorNormal>();
        size = GetComponentInChildren<Size>();
        lifeManager.suscribeDeath(cleanup);
    }

    void syncLookAngle(float oldAngle, float newAngle)
    {
        if (Mathf.Abs(oldAngle = newAngle) % 180 > syncAngleHard)
        {
            currentLookAngle = newAngle;
        }
        else
        {
            currentLookAngle = oldAngle;
        }
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

    public void OrderedUpdate()
    {
        if (!lifeManager.IsDead)
        {
            movement.tick();
        }
        else
        {
            planarVelocityCalculated = Vector3.zero;
        }
    }
    public void OrderedTransition()
    {
        if (!lifeManager.IsDead)
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

    public Vector3 lookWorldPos
    {
        get
        {
            return transform.position + input.lookOffset;
        }
    }
    public Vector3 planarVelocityCalculated
    {
        get
        {
            Vector3 plane3 = Vector3.ProjectOnPlane(rb.velocity, ground.normal);
            Quaternion rot = Quaternion.AngleAxis(-Vector3.Angle(ground.normal, Vector3.up), Vector3.Cross(Vector3.up, ground.normal));
            return rot * plane3;
        }
        set
        {
            Vector3 plane3 = Vector3.ProjectOnPlane(rb.velocity, ground.normal);
            Vector3 vertdiff = rb.velocity - plane3;

            Vector3 move3 = new Vector3(value.x, 0, value.z);
            Quaternion rot = Quaternion.AngleAxis(Vector3.Angle(ground.normal, Vector3.up), Vector3.Cross(Vector3.up, ground.normal));
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


        Vector3 planarVelocity = planarVelocityCalculated;
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
            planarVelocityCalculated = desiredVeloicity;

        }
        else
        {
            planarVelocityCalculated = planarVelocity + diff.normalized * addingFrameMag;
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
        planarVelocityCalculated = desiredDirection * dashSpeed;

    }
    public void setToWalkSpeed()
    {
        Vector3 planarVelocity = planarVelocityCalculated;
        float lookMultiplier = toMoveMultiplier(vec2input(planarVelocity));
        float airMultiplier = 1.0f;
        if (!grounded)
        {
            airMultiplier = 0.6f;
        };

        float potentialSpeed = props.maxSpeed * lookMultiplier * airMultiplier * power.scale();

        planarVelocityCalculated = planarVelocity.normalized * potentialSpeed;
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
    void setGround()
    {
        FloorNormal.GroundSearchParams paras;
        if (size.colliderRef)
        {
            paras = new FloorNormal.GroundSearchParams
            {
                radius = size.scaledRadius,
                distance = size.scaledHalfHeight,
            };
        }
        else
        {
            paras = new FloorNormal.GroundSearchParams
            {
                radius = 0,
                distance = 0,
            };
        }
        ground.setGround(paras);
    }

    public bool grounded
    {
        get
        {

            if (ground && ground.hasGround)
            {
                float mag = Vector3.Dot(rb.velocity, ground.normal);
                return mag <= 0.05f * power.scale();
            }
            return false;

        }
    }


}
