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

public class Projectile : NetworkBehaviour
{
    public GameObject playerHit;
    public GameObject terrainHit;
    public GameObject visualScale;

    public static readonly float BaseProjectileLifetime = 1.5f;

    float lifetime;
    float birth;
    //bool hasHit = false;

    UnitMovement mover;
    HitInstanceData hitData;
    BuffInstanceData buffData;
    HitList hitList;

    [SyncVar]
    ProjectileData data;


    List<Collider> collided = new List<Collider>();
    private void Start()
    {
        birth = Time.time;
        if (isClientOnly)
        {
            setup();
        }
    }
    private void FixedUpdate()
    {
        if (isServer && Time.time > birth + lifetime)
        {
            fireExplode(transform.position + transform.forward * data.hitboxRadius);
        }
    }
    struct ProjectileData
    {
        public float terrainRadius;
        public float hitboxRadius;
        public CapsuleSize sizeC;
        public float length;
        public float range;
        public uint team;
        public float power;
        public float powerByStrength;
        public int visualIndex;
        public AudioDistances dists;
        public float lifetime;

    }
    [Server]
    public void init(float terrainRadius, float hitboxRadius, CapsuleSize sizeC, UnitMovement m, HitInstanceData hitD, BuffInstanceData buffD, HitList hitL, AudioDistances dists)
    {
        mover = m;
        Power p = mover.GetComponent<Power>();
        hitData = hitD;
        buffData = buffD;
        hitList = hitL;
        data = new ProjectileData
        {
            terrainRadius = terrainRadius,
            hitboxRadius = hitboxRadius,
            sizeC = sizeC,
            team = mover.GetComponent<TeamOwnership>().getTeam(),
            power = p.power,
            length = hitData.length,
            range = hitData.range,
            powerByStrength = hitData.powerByStrength,
            visualIndex = hitData.flair.visualIndex,
            dists = dists,
            lifetime = BaseProjectileLifetime / p.scaleTime(),
        };
        setup();
        lifetime = data.lifetime;
        setSpeed(data.range / lifetime);
    }

    void setup()
    {
        float terrainR = data.terrainRadius;
        float hitR = data.hitboxRadius;
        terrainHit.transform.localScale = Vector3.one *terrainR *2 ;
        playerHit.transform.localScale = Vector3.one * hitR * 2; ;
        setAudioDistances(Instantiate(FindObjectOfType<GlobalPrefab>().projectileAssetsPre[data.visualIndex], visualScale.transform), data.dists);
    }

    [Server]
    void setSpeed(float speed)
    {
        GetComponent<Rigidbody>().velocity = transform.forward * speed;
    }

    public void onPlayerCollide(Collider other)
    {
        if (isServer && !collided.Contains(other))
        {

            //collided.Add(other);
            //if (hit(other.gameObject, mover, hitData, data.team, data.power, new KnockBackVectors { center = transform.position, direction = transform.forward }))
            //{
            //    if (buffData != null && buffData.type == BuffType.Debuff)
            //    {
            //        SpawnBuff(buffData, other.GetComponentInParent<BuffManager>().transform);
            //    }
            //    if (!hasHit)
            //    {
            //        hasHit = true;
            //        birth = Time.time;
            //        lifetime = data.lifetime / 3f;
            //        setSpeed(data.length / lifetime);
            //    }

            //}
            if(other.GetComponentInParent<TeamOwnership>().getTeam() != data.team)
            {
                Vector3 diff = other.transform.position - transform.position;
                Vector3 offset = diff.normalized * Mathf.Min(diff.magnitude, data.hitboxRadius);

                fireExplode(transform.position + offset);
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
        return AttackUtils.getShapeData(hitData.shape, data.sizeC, hitData.range, hitData.length, hitData.width, false);
    }

    public void onTerrainCollide(Collider other)
    {
        if (isServer)
        {
            Destroy(gameObject);
        }

    }
}
