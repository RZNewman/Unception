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

public class Projectile : NetworkBehaviour
{
    public GameObject playerHit;
    public GameObject terrainHit;
    public GameObject visualScale;

    public static readonly float BaseProjectileLifetime = 1.5f;

    float lifetime;
    float birth;
    bool hasHit = false;

    UnitMovement mover;
    HitInstanceData hitData;
    BuffInstanceData buffData;

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
            Destroy(gameObject);
        }
    }
    struct ProjectileData
    {
        public float terrainRadius;
        public float hitboxRadius;
        public float halfHeight;
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
    public void init(float terrainRadius, float hitboxRadius, float halfHeight, UnitMovement m, HitInstanceData hitD, BuffInstanceData buffD, AudioDistances dists)
    {
        mover = m;
        Power p = mover.GetComponent<Power>();
        hitData = hitD;
        buffData = buffD;
        data = new ProjectileData
        {
            terrainRadius = terrainRadius,
            hitboxRadius = hitboxRadius,
            halfHeight = halfHeight,
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
        terrainHit.transform.localScale = new Vector3(terrainR, terrainR, terrainR) * 2;
        playerHit.transform.localScale = new Vector3(hitR, attackHitboxHalfHeight(HitType.Projectile, data.halfHeight, hitR) / 2, hitR) * 2;
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

            collided.Add(other);
            if (hit(other.gameObject, mover, hitData, data.team, data.power, new KnockBackVectors { center = transform.position, direction = transform.forward }))
            {
                if (buffData != null && buffData.type == BuffType.Debuff)
                {
                    SpawnBuff(buffData, other.GetComponentInParent<BuffManager>().transform);
                }
                if (!hasHit)
                {
                    hasHit = true;
                    birth = Time.time;
                    lifetime = data.lifetime / 3f;
                    setSpeed(data.length / lifetime);
                }

            }
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
