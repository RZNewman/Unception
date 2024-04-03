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

public class Persistent : NetworkBehaviour
{
    public GameObject playerHit;
    public GameObject terrainHit;
    public GameObject lineVisuals;
    public GameObject circleVisuals;

    public static readonly float ExplodeProjectileLifetime = 1.5f;
    public static readonly float WaveProjectileLifetime = 2.5f;

    float lifetime;
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

    Rigidbody rb;
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
        if (isServer  && Time.time > birth + lifetime)
        {
            if (isProjectileExplode)
            {
                fireExplode(transform.position + transform.forward * data.hitRadius);
            }
            if (isProjectileWave)
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

    bool isProjectileExplode
    {
        get
        {
            return data.hitDataCaptured.type == HitType.ProjectileExploding;
        }
    }
    bool isProjectileWave
    {
        get
        {
            return data.hitDataCaptured.type == HitType.ProjectileWave;
        }
    }
    [Server]
    public void init(CapsuleSize sizeC, UnitMovement m, HitInstanceData hitD, BuffInstanceData buffD, HitList hitL,MoveMode moveT, AudioDistances dists)
    {
        mover = m;
        Power p = mover.GetComponent<Power>();
        hitData = hitD;
        buffData = buffD;
        hitList = hitL;
        moveType = moveT;


        data = new ProjectileData
        {
            sizeC = sizeC,
            team = mover.GetComponent<TeamOwnership>().getTeam(),
            power = p.power,
            hitDataCaptured = new NetworkHitData(hitD),
            dists = dists,
        };
        setup();
        
        if (isProjectileExplode || isProjectileWave)
        {
            lifetime = isProjectileExplode ? ExplodeProjectileLifetime : WaveProjectileLifetime / p.scaleTime();
            setSpeed(hitD.range / lifetime);
        }
        
    }

    void setup()
    {
        if (isProjectileExplode || isProjectileWave)
        {
            terrainHit.transform.localScale = Vector3.one * data.terrainRadius * 2;          
        }
        else
        {
            terrainHit.SetActive(false);
        }
        ShapeData shapeD;
        playerHit.GetComponent<CompoundCollider>().setCallback(onPlayerCollide);
        if (isProjectileExplode)
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
                }
            };
            buildCompoundCollider(shapeD);
            ShapeParticle(transform.rotation, transform.position, transform.forward, shapeD, data.hitDataCaptured.shape, data.hitDataCaptured.flair, mover.sound.dists, transform);
        }
        else if (isProjectileWave)
        {
            shapeD = getShapeData();
            buildCompoundCollider(shapeD);
            ShapeParticle(transform.rotation, transform.position, transform.forward, shapeD, data.hitDataCaptured.shape, data.hitDataCaptured.flair, mover.sound.dists, transform);
        }
        else
        {
            shapeD = getShapeData();
            buildCompoundCollider(shapeD);
            ShapeParticle(transform.parent.GetComponent<SpellSource>(), shapeD, data.hitDataCaptured.shape, data.hitDataCaptured.flair, mover.sound.dists, transform);
        }
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
                    h.transform.localScale = box.size;
                    b.isTrigger = true;
                    break;
                case SphereInfo sphere:
                    SphereCollider s = h.AddComponent<SphereCollider>();
                    h.transform.localScale = Vector3.one * sphere.radius * 2;
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

    public void onPlayerCollide(Collider other)
    {
        if (isServer)
        {

            if (other.GetComponentInParent<TeamOwnership>().getTeam() != data.team)
            {

                switch (data.hitDataCaptured.type)
                {
                    case HitType.ProjectileExploding:
                        Vector3 diff = other.transform.position - transform.position;
                        Vector3 offset = diff.normalized * Mathf.Min(diff.magnitude, data.hitRadius);

                        fireExplode(transform.position + offset);
                        break;
                    default:
                        if (hit(other.gameObject, mover, hitData, data.team, data.power, new KnockBackVectors { center = transform.position, direction = transform.forward },hitList))
                        {
                            if (buffData != null && buffData.type == BuffType.Debuff)
                            {
                                BuffManager bm = other.GetComponentInParent<BuffManager>();
                                if (bm)
                                {
                                    SpawnBuff(buffData, bm.transform);
                                }
                            }
                        }
                        break;
                }
                
            }
            
        }

    }

    [Server]
    void fireExplode(Vector3 contact)
    {
        List<GameObject> hits;
        List<GameObject> enemyHits = new List<GameObject>();
        GroundResult calc = FloorNormal.getGroundNormal(contact, data.sizeC);
        Quaternion aim = Quaternion.LookRotation(transform.forward, calc.normal);
        hits = ShapeAttack(aim, contact, getShapeData());
        foreach (GameObject o in hits)
        {
            if (hit(o, mover, hitData,
                data.team,
                data.power,
                new KnockBackVectors
                {
                    center = contact,
                    direction = transform.forward
                }, hitList))
            {
                enemyHits.Add(o);
            }

        }
        if (buffData != null && buffData.type == BuffType.Debuff)
        {
            foreach (GameObject h in enemyHits)
            {
                BuffManager bm = h.GetComponentInParent<BuffManager>();
                if (bm)
                {
                    SpawnBuff(buffData, bm.transform);
                }
            }

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
}
