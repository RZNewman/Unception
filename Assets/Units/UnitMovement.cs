using Mirror;
using System;
using UnityEngine;
using static AnimationController;
using static AttackUtils;
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
    EventManager events;
    StateMachine<PlayerMovementState> movement;
    Size size;
    Power power;
    FloorNormal ground;
    Combat combat;
    UnitSound _sound;
    Posture posture;
    Mezmerize mezmerize;
    Knockdown knockdown;
    LocalPlayer localPlayer;
    StatHandler statHandler;

    public float syncAngleHard = 40;
    [HideInInspector]
    [SyncVar(hook = nameof(syncLookAngle))]
    public float currentLookAngle = 0;

    [HideInInspector]
    [SyncVar]
    public float currentLookVerticalAngle = 0;

    Vector3 planarVelocityCache;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        power = GetComponent<Power>();
        controller = GetComponent<ControlManager>();
        movement = new StateMachine<PlayerMovementState>(() => new FreeState(this));
        lifeManager = GetComponent<LifeManager>();
        events = GetComponent<EventManager>();
        propHolder = GetComponent<UnitPropsHolder>();
        ground = GetComponent<FloorNormal>();
        size = GetComponentInChildren<Size>();
        combat = GetComponent<Combat>();
        _sound = GetComponent<UnitSound>();
        posture = GetComponent<Posture>();
        mezmerize = GetComponent<Mezmerize>();
        knockdown = GetComponent<Knockdown>();
        localPlayer = GetComponent<LocalPlayer>();
        statHandler = GetComponent<StatHandler>();
        events.suscribeDeath(cleanup);
        events.TransitionEvent += (transition);
        events.TickEvent += (tick);
        events.CastEvent += cast;
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
        return GetComponentInChildren<UnitEye>().gameObject;
    }

    public GameObject getRotationBody()
    {
        return GetComponentInChildren<UnitRotation>().gameObject;
    }

    void tick()
    {
        if (!lifeManager.IsDead)
        {
            if (grounded)
            {
                legMode = LegMode.Normal;
            }
            movement.tick();
        }
        else
        {
            planarVelocityCalculated = Vector3.zero;
        }
    }
    void transition()
    {
        if (!lifeManager.IsDead)
        {
            setGround();
            movement.transition();
        }

    }

    void cast(Ability a)
    {
    }
    void cleanup(bool natural)
    {
        movement.exit();
    }
    public PlayerMovementState currentState()
    {
        return movement.state();
    }

    public bool floating()
    {
        return legMode == LegMode.Float;
    }

    public bool dashing()
    {
        return movement.state() switch
        {
            DashState => true,
            AttackingState => true,
            _ => false
        };
    }
    public enum LegMode
    {
        Normal,
        Float
    }
    public LegMode legMode = LegMode.Normal;

    public void toggleFloat()
    {
        legMode = legMode switch
        {
            LegMode.Float => LegMode.Normal,
            _ => LegMode.Float,
        };
    }
    public string currentAbilityName()
    {
        if (currentState() is AttackingState)
        {
            return ((AttackingState)currentState()).abilityName;
        }
        return "";
    }
    public Optional<AttackSegment> currentAttackSegment()
    {
        if (currentState() is AttackingState)
        {
            return ((AttackingState)currentState()).segment;
        }
        return null;
    }

    public Optional<Ability> currentAttackingAbility()
    {
        if (currentState() is AttackingState)
        {
            return ((AttackingState)currentState()).currentAbility;
        }
        return new Optional<Ability>();
    }


    public bool isIncapacitated
    {
        get
        {
            return posture.isStunned || mezmerize.isMezmerized || knockdown.knockedDown;
        }
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
            Vector3 plane3New = vectorInPlane(move3);
            rb.velocity = plane3New + vertdiff;
        }
    }

    Vector3 vectorInPlane(Vector3 vec)
    {
        Quaternion rot = Quaternion.AngleAxis(Vector3.Angle(ground.normal, Vector3.up), Vector3.Cross(Vector3.up, ground.normal));
        return rot * vec;
    }

    public void tryJump()
    {
        if (grounded)
        {
            jump();
        }
        else if(props.canFloat)
        {
            toggleFloat();
        }
    }

    public void jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, props.jumpForce * power.scalePhysical(), rb.velocity.z);
    }
    public void knock(Vector3 dir, float back, float up)
    {
        dir.Normalize();
        if (posture.isStunned)
        {
            back *= 1.1f;
            up *= 1.1f;
        }
        knockdown.tryKnockDown(back, up);
        applyForce(dir * back);
        applyForce(Vector3.up * up);
    }

    void applyForce(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
    }

    public void move(UnitInput inp, float speedStateMultiplier = 1.0f, float additionalMovement = 0)
    {
        float scaleSpeed = power.scaleSpeed();


        Vector3 planarVelocity = planarVelocityCalculated;
        Vector3 desiredDirection = input2vec(inp.move);
        float movespeed = Mathf.Max(props.maxSpeed + additionalMovement + statHandler.getValue(Stat.Movespeed, power.scaleNumerical()), 0);
        float movespeedMult = speedStateMultiplier * (isIncapacitated ? knockdown.knockedDown ? 0 : 0.2f : 1.0f) * (grounded ? 1.0f : 0.8f) * (combat.inCombat ? 1.0f : 1.5f) * (floating() ? 0.7f : 1.0f);
        float frictionMult = (grounded ? 1.0f : 0.3f) * (floating() ? 6.0f : 1.0f) * (isIncapacitated ? 0.6f : 1.0f);
        float stoppingMult = speedStateMultiplier * (isIncapacitated ? knockdown.knockedDown ? 0 : 0.2f : 1.0f) * (grounded ? 1.0f : 0.3f) * (combat.inCombat ? 1.0f : 1.5f) * (floating() ? 6.0f : 1.0f);
        float accelerationMult = speedStateMultiplier * (isIncapacitated ? knockdown.knockedDown ? 0 : 0.2f : 1.0f) * (grounded ? 1.0f : 0.3f) * (combat.inCombat ? 1.0f : 1.5f);


        float maxSpeedFriction = movespeed * movespeedMult * toMoveMultiplier(vec2input(planarVelocity)) * scaleSpeed;
        float frictionFrameMag = props.friction * frictionMult * Time.fixedDeltaTime * scaleSpeed;
        float potentialSpeed = movespeed * movespeedMult * toMoveMultiplier(inp.move) * scaleSpeed;
        float stoppingFrameMag = props.decceleration * stoppingMult * Time.fixedDeltaTime * scaleSpeed;
        //the look mulitplier for acceleration is applied later
        float accelerationFrameMag = props.acceleration * accelerationMult * Time.fixedDeltaTime * scaleSpeed;


        //section: friction
        if (planarVelocity.magnitude > maxSpeedFriction)
        {
            //apply fricton
            float frictionMagDiff = planarVelocity.magnitude - maxSpeedFriction;
            if (frictionFrameMag >= frictionMagDiff)
            {
                planarVelocity = planarVelocity.normalized * maxSpeedFriction;
            }
            else
            {
                planarVelocity += -planarVelocity.normalized * frictionFrameMag;
            }
        }


        //section: stopping
        float usefulSpeed = Mathf.Max(Vector3.Dot(planarVelocity, desiredDirection), 0);
        float desiredSpeed = Mathf.Max(usefulSpeed, potentialSpeed);
        Vector3 desiredVeloicity = desiredDirection * desiredSpeed;
        Vector3 stoppingDiff = desiredVeloicity - planarVelocity;
        float stoppingMagnitude = Vector3.Dot(stoppingDiff, -planarVelocity);
        stoppingMagnitude = Mathf.Max(stoppingMagnitude, 0);
        Vector3 stoppingVec = -planarVelocity.normalized * stoppingMagnitude;
        if (stoppingVec.magnitude <= stoppingFrameMag)
        {
            planarVelocity += stoppingVec;

        }
        else
        {
            planarVelocity += stoppingVec.normalized * stoppingFrameMag;
        }

        //section: acceleration
        Vector3 accelerationDiff = desiredVeloicity - planarVelocity;
        float lookMultiplierAcc = toMoveMultiplier(vec2input(accelerationDiff));
        accelerationFrameMag *= lookMultiplierAcc;
        if (accelerationDiff.magnitude <= accelerationFrameMag)
        {
            planarVelocity = desiredVeloicity;

        }
        else
        {
            planarVelocity += accelerationDiff.normalized * accelerationFrameMag;
        }
        planarVelocityCache = planarVelocity;
        planarVelocityCalculated = planarVelocityCache;

    }

    public DashInstanceData baseDash()
    {
        float combatMultiplier = 1.0f;
        float airMultiplier = 1.0f;
        float pitch = 0;
        if (!combat.inCombat)
        {
            combatMultiplier = 1.5f;
        }
        if (!grounded)
        {
            airMultiplier = 0.5f;
            pitch = -45;
        }

        float scalePhys = power.scalePhysical();
        float scaleSpeed = power.scaleSpeed();
        return new DashInstanceData
        {
            distance = props.dashDistance * airMultiplier * scalePhys,
            speed = props.dashSpeed * airMultiplier * combatMultiplier * scaleSpeed,
            control = DashControl.Input,
            endMomentum = DashEndMomentum.Walk,
            pitch = pitch,
        };
    }
    public void dash(UnitInput inp, DashInstanceData opts)
    {
        Vector3 desiredDirection;
        switch (opts.control)
        {
            case DashControl.Forward:
                desiredDirection = getRotationBody().transform.forward;
                break;
            case DashControl.Backward:
                desiredDirection = -getRotationBody().transform.forward;
                break;
            case DashControl.Input:
                desiredDirection = input2vec(inp.move);
                break;
            default:
                desiredDirection = Vector3.zero;
                break;
        }
        if (opts.pitch > 0 || (!grounded && opts.pitch < 0))
        {
            Quaternion rot = Quaternion.AngleAxis(opts.pitch, Vector3.Cross(desiredDirection, Vector3.up));
            desiredDirection = rot * desiredDirection;
        }
        rb.velocity = vectorInPlane(desiredDirection * opts.speed);

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

        float potentialSpeed = Mathf.Max(props.maxSpeed + statHandler.getValue(Stat.Movespeed, power.scaleNumerical()), 0) * lookMultiplier * airMultiplier * combatMultiplier * power.scaleSpeed();

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
        float airMultiplier = 1.0f;
        if (!grounded)
        {
            airMultiplier = 0.6f;
        }
        //degrees in proportial to the world right now, but if the player is bigger, we need to reduce it
        additionalRotationDegrees /= power.scalePhysical();
        canSnap &= props.isPlayer;
        float turnSpeed = canSnap ? 180f / Time.fixedDeltaTime : Mathf.Max(props.lookSpeedDegrees + additionalRotationDegrees + statHandler.getValue(Stat.Turnspeed, power.scaleNumerical()), 0);

        //horizontal angle
        float desiredAngle = -Vector2.SignedAngle(Vector2.up, inp.look);
        float diff = desiredAngle - currentLookAngle;
        float frameMagnitude = turnSpeed * speedMultiplier * airMultiplier * Time.fixedDeltaTime;
        diff = normalizeAngle(diff);
        if (Mathf.Abs(diff) <= frameMagnitude)
        {
            currentLookAngle = desiredAngle;
        }
        else
        {
            currentLookAngle += frameMagnitude * Mathf.Sign(diff);
        }

        //verticalAngle
        desiredAngle = inp.lookVerticalAngle;     
        diff = desiredAngle - currentLookVerticalAngle;
        if (Mathf.Abs(diff) <= frameMagnitude)
        {
            currentLookVerticalAngle = desiredAngle;
        }
        else
        {
            currentLookVerticalAngle += frameMagnitude * Mathf.Sign(diff);
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
        ground.setGround(size.sizeC);
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
