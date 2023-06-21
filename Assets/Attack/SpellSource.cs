using JetBrains.Annotations;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackUtils;
using static GenerateDash;
using static GenerateHit;
using static IndicatorHolder;
using static Size;

public class SpellSource : NetworkBehaviour, IndicatorHolder, TeamOwnership
{

    [SyncVar]
    public Vector3 target;
    [SyncVar]
    public float speed;

    Rigidbody rb;

    [SyncVar]
    CapsuleSize sizeC;

    [SyncVar]
    GameObject owner;

    [SyncVar]
    public float offsetMult = 1f;

    FloorNormal ground;
    GlobalPrefab global;

    [SyncVar]
    MoveMode moveType;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ground = GetComponent<FloorNormal>();
        global = FindObjectOfType<GlobalPrefab>();
    }
    private void Start()
    {
        switch (moveType)
        {
            case MoveMode.Parent:
                //GetComponent<NetworkTransform>().enabled = false;
                GetComponent<Mirror.Experimental.NetworkRigidbody>().enabled = false;
                Destroy(rb);
                break;
        }
        owner.GetComponent<Cast>().addSource(this);
        if (isClientOnly)
        {
            owner.GetComponent<UnitMovement>().currentAttackSegment().Value.clientSyncSource(this);
        }
    }
    private void OnDestroy()
    {
        owner.GetComponent<Cast>().removeSource(this);
    }

    public void OrderedUpdate()
    {
        ground.setGround(sizeC);
        AttackSegment? seg = owner.GetComponent<UnitMovement>().currentAttackSegment();
        if (seg.HasValue)
        {
            seg.Value.IndicatorUpdate();
        }
    }
    public void updateIndicator(IndicatorType type, IndicatorOffsets offsets)
    {
        IndicatorInstance ind = indicators[type];
        ind.setLocalOffsets(offsets);
        ind.OrderedUpdate();
    }



    public CapsuleSize sizeCapsule
    {
        get
        {
            return sizeC;
        }
    }
    public enum MoveMode : byte
    {
        Parent,
        World,
    }
    public void init(CapsuleSize sizeCap, GameObject own, uint teamNum, MoveMode type)
    {
        owner = own;
        sizeC = sizeCap;
        team = teamNum;
        moveType = type;
    }


    public Vector3 indicatorPosition(Vector3 forward)
    {
        return sizeC.indicatorPosition(forward);
    }
    public float offsetMultiplier()
    {
        return offsetMult;
    }
    public IndicatorLocalLook pointOverride(Vector3 forwardPlanar, Vector3 groundNormal)
    {
        if (offsetMult == 0)
        {
            return new IndicatorLocalLook
            {
                shouldOverride = false,
            };
        }
        return sizeC.pointOverride(transform, forwardPlanar, groundNormal);
    }

    public void setTarget(Vector3 t, float s)
    {
        target = t;
        speed = s;
        moveTowardTarget();
    }

    public void moveTowardTarget()
    {

        Vector3 diff = target - transform.position;
        float frameDistance = speed * Time.fixedDeltaTime;
        if (diff.magnitude < frameDistance)
        {
            transform.position = target;
            rb.velocity = Vector3.zero;
        }
        else
        {
            rb.velocity = speed * diff.normalized;
        }


    }



    public struct IndicatorOffsets
    {
        public float time;
        public Vector3 distance;

        public IndicatorOffsets sum(IndicatorOffsets b)
        {
            return new IndicatorOffsets
            {
                time = this.time + b.time,
                distance = this.distance + b.distance,
            };
        }
    }
    public enum IndicatorType
    {
        Hit,
        Dash
    }
    Dictionary<IndicatorType, IndicatorInstance> indicators = new Dictionary<IndicatorType, IndicatorInstance>();
    public void buildHitIndicator(HitInstanceData data)
    {
        GameObject prefab = data.type switch
        {
            HitType.Line => global.LineIndPre,
            HitType.Projectile => global.ProjIndPre,
            HitType.Ground => global.GroundIndPre,
            _ => global.LineIndPre
        };
        GameObject indicator = Instantiate(
        prefab,
            transform
        );
        HitIndicatorInstance i = indicator.GetComponent<HitIndicatorInstance>();
        i.setSource(data);
        i.setTeam(team);
        indicators.Add(IndicatorType.Hit, i);
    }

    public void buildDashIndicator(DashInstanceData data)
    {
        Power pow = owner.GetComponent<Power>();
        GameObject indicator = Instantiate(
        global.DashIndPre,
            transform
        );
        DashIndicatorVisuals d = indicator.GetComponent<DashIndicatorVisuals>();
        d.setSource(data, pow);
        d.setTeam(team);
        indicators.Add(IndicatorType.Dash, d);
    }
    public void killIndicatorType(IndicatorType type)
    {
        Destroy(indicators[type].gameObject);
        indicators.Remove(type);

    }

    uint team;

    public uint getTeam()
    {
        return team;
    }
}
