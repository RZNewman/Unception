using JetBrains.Annotations;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackUtils;
using static GenerateDash;
using static GenerateHit;
using static IndicatorHolder;
using static Size;
using static Utils;

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

    [SyncVar]
    public Vector3 movementOffset = Vector3.zero;

    [SyncVar]
    public Quaternion multipleRotation = Quaternion.identity;

    FloorNormal ground;
    GlobalPrefab global;

    [SyncVar]
    MoveMode moveType;

    AttackSegment segment;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ground = GetComponent<FloorNormal>();
        global = GlobalPrefab.gPre;
    }
    private void Start()
    {
        switch (moveType)
        {
            case MoveMode.Parent:
                GetComponent<NetworkTransformUnreliable>().enabled = false;
                GetComponent<NetworkRigidbodyUnreliable>().enabled = false;
                Destroy(rb);
                break;
        }
        if (isClientOnly)
        {
            //TODO cant get segment for triggered abils
            //owner.GetComponent<UnitMovement>().currentAttackSegment().Value.clientSyncSource(this);
        }
    }

    public void PreUpdate()
    {
        ground.setGround(sizeC);
    }
    public void updateIndicator(int id, IndicatorOffsets offsets, float selfPercent)
    {
        if(!indicators.ContainsKey(id))
        {
            //expected when using multiple projectiles
            return;
        }
        IndicatorInstance ind = indicators[id];
        offsets.distance = Quaternion.Inverse(multipleRotation) * offsets.distance;
        ind.setLocalOffsets(offsets,selfPercent);
        ind.OrderedUpdate();
    }


    public Quaternion aimRotation()
    {
        UnitEye eye = GetComponentInParent<UnitEye>();

        if (eye)
        {
            //rotation handled by unit eye; default to foward
            return Quaternion.LookRotation(transform.forward, ground.normal);
        }
        else
        {
            return ground.getAimRotation(transform.forward);
        }
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

    public void setTarget(Vector3 t, float s)
    {
        target = t + movementOffset;
        speed = s;
        moveTowardTarget();
    }

    public void moveTowardTarget()
    {
        Vector3 diffFull = target - transform.position;
        Vector3 diffFlat = Vector3.ProjectOnPlane(diffFull, ground.normal);
        float frameDistance = speed * Time.fixedDeltaTime;
        Vector3 velo = Vector3.zero;
        if (diffFlat.magnitude < frameDistance)
        {
            transform.position += diffFlat;
        }
        else
        {
            velo = speed * diffFlat.normalized;
        }

        Vector3 diffPerp = diffFull - diffFlat;
        if(diffFlat.magnitude <= speed * 0.5f)
        {
            if (diffPerp.magnitude < frameDistance)
            {
                transform.position += diffPerp;
            }
            else
            {
                velo += speed * diffPerp.normalized;
            }
        }

        rb.velocity = velo;
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

    Dictionary<int, IndicatorInstance> indicators = new Dictionary<int, IndicatorInstance>();
    public int buildHitIndicator(HitInstanceData data, ShapeData shapeData)
    {
        GameObject prefab = data.type switch
        {
            HitType.Attached => global.ShapeIndPre,
            HitType.ProjectileExploding => global.ProjIndPre,
            HitType.GroundPlaced => global.ShapeIndPre,
            HitType.DamageDash => global.ShapeIndPre,
            _ => global.LineIndPre
        };
        GameObject indicator = Instantiate(
        prefab,
            transform
        );
        HitIndicatorInstance i = indicator.GetComponent<HitIndicatorInstance>();
        i.setSource(data, shapeData);
        i.setTeam(team);
        int id = indicator.GetInstanceID();
        indicators.Add(id,i);
        return id;
    }

    public int buildDashIndicator(DashInstanceData data)
    {
        Power pow = owner.GetComponent<Power>();
        GameObject indicator = Instantiate(
        global.DashIndPre,
            transform
        );
        DashIndicatorVisuals d = indicator.GetComponent<DashIndicatorVisuals>();
        d.setSource(data, pow);
        d.setTeam(team);
        int id = indicator.GetInstanceID();
        indicators.Add(id, d);
        return id;
    }
    public void killIndicator(int id)
    {
        if (!indicators.ContainsKey(id))
        {
            //expected when using multiple projectiles
            return;
        }
        Destroy(indicators[id].gameObject);
        indicators.Remove(id);

    }

    uint team;

    public uint getTeam()
    {
        return team;
    }
}
