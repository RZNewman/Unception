using Mirror;
using System;
using UnityEngine;
using static AnimationController;
using static DashState;
using static GenerateDash;
using static StatTypes;
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
    Combat combat;
    UnitSound _sound;
    Posture _posture;
    LocalPlayer localPlayer;
    StatHandler statHandler;

    public float syncAngleHard = 40;
    [HideInInspector]
    [SyncVar(hook = nameof(syncLookAngle))]
    public float currentLookAngle = 0;

    Vector3 planarVelocityCache;
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
        combat = GetComponent<Combat>();
        _sound = GetComponent<UnitSound>();
        _posture = GetComponent<Posture>();
        localPlayer = GetComponent<LocalPlayer>();
        statHandler = GetComponent<StatHandler>();
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
    void cleanup(bool natural)
    {
        movement.exit();
    }
    public PlayerMovementState currentState()
    {
        return movement.state();
    }

    public Posture posture
    {
        get { return _posture; }
    }

    public UnitInput input
    {
        get
        {
            return controller.GetUnitInput();
        }
    }
    public UnitSound sound
    {
        get
        {
            return _sound;
        }
    }

    public LocalPlayer local
    {
        get
        {
            return localPlayer;
        }
    }
    public Vector3 lookWorldPos
    {
        get
        {
            return transform.position + input.lookOffset;
        }
    }
    public Vector3 planarVelocity
    {
        get
        {
            return planarVelocityCache;
        }
    }

    public Vector3 worldVelocity
    {
        get
        {
            return rb.velocity;
        }
    }

    public MoveDirection moveDirection
    {
        get
        {
            if (planarVelocityCache.magnitude < 0.05f)
            {
                return MoveDirection.None;
            }
            float angle = Vector3.SignedAngle(Vector3.forward, planarVelocityCache, Vector3.up);
            float diff = angle - currentLookAngle;
            diff = normalizeAngle(diff);
            if (Mathf.Abs(diff) < 45)
            {
                return MoveDirection.Forward;
            }
            if (Mathf.Abs(diff) > 135)
            {
                return MoveDirection.Backward;
            }
            if (diff > 0)
            {
                return MoveDirection.Right;
            }
            else
            {
                return MoveDirection.Left;
            }

        }
    }
    Vector3 planarVelocityCalculated
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
        rb.velocity = new Vector3(rb.velocity.x, props.jumpForce * power.scalePhysical(), rb.velocity.z);
    }
    public void applyForce(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
    }

    public void move(UnitInput inp, float speedMultiplier = 1.0f, float additionalMovement = 0)
    {
        float scaleSpeed = power.scaleSpeed();

        float lookMultiplier = toMoveMultiplier(inp.move);
        float airMultiplier = 1.0f;
        float combatMultiplier = 1.0f;
        float stunnedMultiplier = 1.0f;
        if (!grounded)
        {
            airMultiplier = 0.6f;
        }
        if (!combat.inCombat)
        {
            combatMultiplier = 1.5f;
        }
        if (posture.isStunned)
        {
            stunnedMultiplier = 0.6f;
        }



        Vector3 desiredDirection = input2vec(inp.move);

        speedMultiplier *= lookMultiplier * airMultiplier * combatMultiplier;


        Vector3 planarVelocity = planarVelocityCalculated;
        float potentialSpeed = (props.maxSpeed + additionalMovement + statHandler.getValue(Stat.Movespeed, power.scaleNumerical())) * speedMultiplier * scaleSpeed;
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
        float stoppingMult = stunnedMultiplier * airMultiplier * combatMultiplier;
        float stoppingFrameMag = props.decceleration * stoppingMult * Time.fixedDeltaTime * scaleSpeed;

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
        float addingMult = speedMultiplier * airMultiplier * combatMultiplier * lookMultiplierDiff;
        float addingFrameMag = props.acceleration * addingMult * Time.fixedDeltaTime * scaleSpeed;

        if (diff.magnitude <= addingFrameMag)
        {
            planarVelocityCache = desiredVeloicity;

        }
        else
        {
            planarVelocityCache = planarVelocity + diff.normalized * addingFrameMag;
        }
        planarVelocityCalculated = planarVelocityCache;

    }

    public DashInstanceData baseDash()
    {
        float combatMultiplier = 1.0f;
        if (!combat.inCombat)
        {
            combatMultiplier = 1.5f;
        }
        float scalePhys = power.scalePhysical();
        float scaleSpeed = power.scaleSpeed();
        return new DashInstanceData
        {
            distance = props.dashDistance * scalePhys,
            speed = props.dashSpeed * combatMultiplier * scaleSpeed,
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
        float combatMultiplier = 1.0f;
        if (!combat.inCombat)
        {
            combatMultiplier = 1.5f;
        }
        if (!grounded)
        {
            airMultiplier = 0.6f;
        };

        float potentialSpeed = props.maxSpeed * lookMultiplier * airMultiplier * combatMultiplier * power.scaleSpeed();

        planarVelocityCalculated = planarVelocity.normalized * potentialSpeed;
    }
    public void stop(bool alsoGravity = false)
    {
        if (alsoGravity)
        {
            rb.velocity = Vector3.zero;
        }
        else
        {
            planarVelocityCalculated = Vector3.zero;
        }

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
    public void rotate(UnitInput inp, bool canSnap = true, float speedMultiplier = 1.0f, float additionalRotationDegrees = 0)
    {
        if (inp.look == Vector2.zero)
        {
            return;
        }
        //degrees in proportial to the world right now, but if the player is bigger, we need to reduce it
        additionalRotationDegrees /= power.scalePhysical();
        canSnap &= props.isPlayer;
        float turnSpeed = canSnap ? 180f / Time.fixedDeltaTime : props.lookSpeedDegrees + additionalRotationDegrees + statHandler.getValue(Stat.Turnspeed, power.scaleNumerical());

        float desiredAngle = -Vector2.SignedAngle(Vector2.up, inp.look);
        float diff = desiredAngle - currentLookAngle;
        float frameMagnitude = turnSpeed * speedMultiplier * Time.fixedDeltaTime;
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
    public void maybeSnapRotation(UnitInput inp)
    {
        if (inp.look == Vector2.zero || !props.isPlayer)
        {
            return;
        }
        float desiredAngle = -Vector2.SignedAngle(Vector2.up, inp.look);
        currentLookAngle = desiredAngle;
        GetComponentInChildren<UnitRotation>().updateRotation();

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
                return mag <= 0.05f * power.scalePhysical();
            }
            return false;

        }
    }


}
