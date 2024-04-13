using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackUtils;
using static GenerateHit;
using Mirror;
using static IndicatorInstance;
using UnityEngine.VFX;
using static UnitSound;
using static GenerateBuff;
using static UnityEngine.Rendering.HableCurve;
using static Size;
using static FloorNormal;
using UnityEngine.UIElements;
using Unity.Burst.Intrinsics;
using static GenerateAttack;
using static AttackUtils.CapsuleInfo;
using static UnityEngine.UI.Image;
using System.Linq;
using System;
using static SpellSource;
using static GenerateHit.HitInstanceData;

public class Persistent : NetworkBehaviour, Duration, IndicatorHolder
{
    public GameObject playerHit;
    public GameObject terrainHit;
    public GameObject lineVisuals;
    public GameObject circleVisuals;

    public static readonly float ExplodeProjectileLifetime = 1.5f;
    public static readonly float WaveProjectileLifetime = 2.5f;

    float lifetime;
    float maxLifetime;
    float birth;
    //bool hasHit = false;

    UnitMovement mover;
    HitInstanceData hitData;
    BuffInstanceData buffData;
    HitList hitList;

    [SyncVar]
    ProjectileData data;

    [SyncVar]
    MoveMode moveType;

    [SyncVar]
    Buff AuraBuff;

    public enum PersistMode
    {
        //default switches to another option
        Default,
        Explode,
        Wave,
        Dash,
        AuraPlaced,
        AuraCarried,
        AuraChanneled,
    }

    [SyncVar]
    PersistMode mode;

    Rigidbody rb;

    public float remainingDuration
    {
        get
        {
            return lifetime;
        }
    }

    public float maxDuration
    {
        get
        {
            return maxLifetime;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

    }

    private void Start()
    {
        birth = Time.time;
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
            setup();
        }
    }
    private void FixedUpdate()
    {
        lifetime -= Time.fixedDeltaTime;
        if (isServer  && lifetime <=0)
        {
            if (mode == PersistMode.Explode)
            {
                fireExplode(transform.position + transform.forward * data.hitRadius);
            }
            if (mode == PersistMode.AuraPlaced || mode == PersistMode.Wave || mode == PersistMode.AuraCarried || mode == PersistMode.AuraChanneled)
            {
                Destroy(gameObject);
            }
            
        }
    }

    struct NetworkHitData
    {
        public float range;
        public float width;
        public float length;
        public EffectShape shape;
        public HitType type;
        public HitFlair flair;

        public NetworkHitData(HitInstanceData source)
        {
            range = source.range;
            width = source.width;
            length = source.length;
            shape = source.shape;
            type = source.type;
            flair = source.flair;
        }
    }

    struct ProjectileData
    {
        public CapsuleSize sizeC;
        public uint team;
        public float power;
        public NetworkHitData hitDataCaptured;
        public AudioDistances dists;

        public float hitRadius
        {
            get
            {
                return hitDataCaptured.width / 4;
            }
        }
        public float terrainRadius
        {
            get
            {
                return Mathf.Min(hitRadius, sizeC.radius * 0.5f);
            }
        }

    }

    [Server]
    public void init(CapsuleSize sizeC, UnitMovement m, HitInstanceData hitD, BuffInstanceData buffD, HitList hitL,MoveMode moveT, AudioDistances dists, PersistMode persistMode)
    {
        mover = m;
        Power p = mover.GetComponent<Power>();
        hitData = hitD;
        buffData = buffD;
        hitList = hitL;
        moveType = moveT;
        mode = persistMode;

        data = new ProjectileData
        {
            sizeC = sizeC,
            team = mover.GetComponent<TeamOwnership>().getTeam(),
            power = p.power,
            hitDataCaptured = new NetworkHitData(hitD),
            dists = dists,
        };
        setup();
        
        if (mode == PersistMode.Explode || mode == PersistMode.Wave)
        {
            maxLifetime = mode == PersistMode.Explode ? ExplodeProjectileLifetime : WaveProjectileLifetime / p.scaleTime();
            lifetime = maxLifetime;
            setSpeed(hitD.range / lifetime);
        }
        if (mode == PersistMode.AuraPlaced || mode == PersistMode.AuraChanneled || mode == PersistMode.AuraCarried)
        {
            maxLifetime = hitData.dotTime;
            lifetime = maxLifetime;
            HarmPortions harm = hitData.getHarmValues(data.power, new KnockBackVectors
            {
                center = transform.position,
                direction = transform.forward
            });
            AuraBuff = SpawnAuraBuff(transform, hitData.scales, this, harm.OverTime.Value);
        }
        
    }

    void setup()
    {
        if (mode == PersistMode.Explode || mode == PersistMode.Wave)
        {
            terrainHit.transform.localScale = Vector3.one * data.terrainRadius * 2;          
        }
        else
        {
            terrainHit.SetActive(false);
        }
        ShapeData shapeD;
        playerHit.GetComponent<CompoundCollider>().setCallback(onPlayerCollide);
        if (mode == PersistMode.Explode)
        {
            shapeD = new ShapeData
            {
                colliders = new List<ColliderInfo>()
                {
                    new SphereInfo
                    {
                        radius = data.hitRadius
                    }
                },             
                vfx = new VFXArcInfo
                {
                    radius = data.hitRadius,
                    arcDegrees = 180,
                    height = data.hitRadius,
                },
                indicators = new List<IndicatorDisplay>(){ new IndicatorDisplay
                {
                    rotation = Quaternion.identity,
                    scale = Vector3.one* data.hitRadius *2,
                    shape = IndicatorShape.Circle,

                } }
            };
            buildCompoundCollider(shapeD);
            buildShapeInd(shapeD);
            ShapeParticle(transform.rotation, transform.position, transform.forward, shapeD, data.hitDataCaptured.shape, data.hitDataCaptured.flair, mover.sound.dists, transform);
        }
        else if (mode == PersistMode.AuraPlaced || mode == PersistMode.Wave || mode == PersistMode.AuraCarried)
        {
            shapeD = getShapeData();
            buildCompoundCollider(shapeD);
            buildShapeInd(shapeD);
            ShapeParticle(transform.rotation, transform.position, transform.forward, shapeD, data.hitDataCaptured.shape, data.hitDataCaptured.flair, mover.sound.dists, transform);
        }
        else
        {
            shapeD = getShapeData();
            buildCompoundCollider(shapeD);
            buildShapeInd(shapeD);
            ShapeParticle(transform.parent.GetComponent<SpellSource>(), shapeD, data.hitDataCaptured.shape, data.hitDataCaptured.flair, mover.sound.dists, transform);
        }
    }

    void buildShapeInd(ShapeData shapeData)
    {
        GlobalPrefab global = FindObjectOfType<GlobalPrefab>();
        GameObject indicator = Instantiate(
            global.ShapeIndPre,
            transform
        );
        HitIndicatorInstance i = indicator.GetComponent<HitIndicatorInstance>();
        i.setSource(hitData, shapeData);
        i.setTeam(data.team);
    }

    void buildCompoundCollider(ShapeData shapeData)
    {
        //GroundResult calc = FloorNormal.getGroundNormal(transform.position, data.sizeC);
        //Quaternion aim = Quaternion.LookRotation(transform.forward, calc.normal);
        foreach (ColliderInfo info in shapeData.colliders)
        {
            //Vector3 totalPosition = transform.position + aim * info.position;
            //Quaternion totalRotation = aim * info.rotation;
            GameObject h = new GameObject("Collider");
            h.layer = LayerMask.NameToLayer("Hitboxes");
            h.transform.parent = playerHit.transform;
            FragmentCollider frag =  h.AddComponent<FragmentCollider>();
            frag.subtract = info.subtract;
            h.transform.localPosition = info.position;
            h.transform.localRotation = info.rotation;
            switch (info)
            {
                case BoxInfo box:
                    BoxCollider b = h.AddComponent<BoxCollider>();
                    b.size = box.size;
                    b.isTrigger = true;
                    break;
                case SphereInfo sphere:
                    SphereCollider s = h.AddComponent<SphereCollider>();
                    s.radius = sphere.radius;
                    s.isTrigger = true;
                    break;
                case CapsuleInfo capsule:
                    CapsuleCollider c = h.AddComponent<CapsuleCollider>();
                    c.direction = (int)capsule.capsuleDir;
                    c.radius =capsule.radius;
                    c.height = capsule.height;
                    c.isTrigger = true;
                    break;
            }
        }

    }

    [Server]
    void setSpeed(float speed)
    {
        GetComponent<Rigidbody>().velocity = transform.forward * speed;
    }

    public void onPlayerCollide(Collider other, bool enter)
    {
        if (isServer)
        {

            if (other.GetComponentInParent<TeamOwnership>().getTeam() != data.team)
            {
                if (enter)
                {
                    switch (mode)
                    {
                        case PersistMode.AuraPlaced:
                        case PersistMode.AuraCarried:
                        case PersistMode.AuraChanneled:
                            other.GetComponentInParent<BuffManager>().addBuff(AuraBuff);
                            break;
                        case PersistMode.Explode:
                            Vector3 diff = other.transform.position - transform.position;
                            Vector3 offset = diff.normalized * Mathf.Min(diff.magnitude, data.hitRadius);

                            fireExplode(transform.position + offset);
                            break;
                        default:
                            HarmPortions harm = hitData.getHarmValues(data.power, new KnockBackVectors
                            {
                                center = transform.position,
                                direction = transform.forward
                            });

                            hit(other.gameObject, mover, harm, data.team, hitList, buffData);

                            break;
                    }
                }
                else
                {
                    switch (mode)
                    {
                        case PersistMode.AuraPlaced:
                        case PersistMode.AuraCarried:
                        case PersistMode.AuraChanneled:
                            other.GetComponentInParent<BuffManager>().removeBuff(AuraBuff);
                            break;
                    }
                }
                
                
            }
            
        }

    }

    private void OnDestroy()
    {
        switch (mode)
        {
            case PersistMode.AuraPlaced:
            case PersistMode.AuraCarried:
            case PersistMode.AuraChanneled:
                foreach (Collider col in playerHit.GetComponent<CompoundCollider>().colliding)
                {
                    if (col)
                    {
                        col.GetComponentInParent<BuffManager>().removeBuff(AuraBuff);
                    }
                    
                }
                break;
        }
    }



    [Server]
    void fireExplode(Vector3 contact)
    {
        List<GameObject> hits;
        GroundResult calc = FloorNormal.getGroundNormal(contact, data.sizeC);
        Quaternion aim = Quaternion.LookRotation(transform.forward, calc.normal);
        hits = ShapeAttack(aim, contact, getShapeData());
        HarmPortions harm = hitData.getHarmValues(data.power, new KnockBackVectors
        {
            center = contact,
            direction = transform.forward
        });
        //Cant channel a projectile
        if (hitData.dotType == DotType.Placed)
        {
            harm.OverTime = null;
            SpawnPersistent(aim, gameObject,data.sizeC, mover, hitData, null, null, mover.sound.dists, PersistMode.AuraPlaced);
        }

        foreach (GameObject o in hits)
        {
            hit(o, mover, harm,
                data.team, hitList, buffData);


        }
        ShapeParticle(aim, contact, transform.forward, getShapeData(), hitData.shape, hitData.flair, mover.sound.dists);
        Destroy(gameObject);
    }

    

    public ShapeData getShapeData()
    {
        if (isServer)
        {
            return AttackUtils.getShapeData(hitData.shape, data.sizeC, hitData.range, hitData.length, hitData.width, usesRangeForHitbox(hitData.type));
        }
        else
        {
            return AttackUtils.getShapeData(data.hitDataCaptured.shape, data.sizeC, data.hitDataCaptured.range, data.hitDataCaptured.length, data.hitDataCaptured.width, usesRangeForHitbox(data.hitDataCaptured.type));
        }
        
    }

    public void onTerrainCollide(Collider other)
    {
        if (isServer)
        {
            Destroy(gameObject);
        }

    }

    public Vector3 indicatorPosition()
    {
        return data.sizeC.indicatorPosition();
    }

    public float offsetMultiplier()
    {
        return 0;
    }
}
