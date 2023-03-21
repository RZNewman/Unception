using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackUtils;
using static GenerateHit;
using Mirror;
using static IndicatorInstance;
using UnityEngine.VFX;
using static UnitSound;

public class Projectile : NetworkBehaviour
{
    public GameObject playerHit;
    public GameObject terrainHit;
    public GameObject visualScale;

    public float speed;

    public static readonly float BaseProjectileLifetime = 1.5f;

    float lifetime;
    float birth;

    UnitMovement mover;

    [SyncVar]
    ProjectileData data;


    List<Collider> collided = new List<Collider>();
    private void Start()
    {
        birth = Time.time;
        if (isClientOnly)
        {
            setup(data);
        }
    }
    private void FixedUpdate()
    {
        if (Time.time > birth + lifetime)
        {
            Destroy(gameObject);
        }
    }
    struct ProjectileData
    {
        public float terrainRadius;
        public float hitboxRadius;
        public float halfHeight;
        public uint team;
        public float power;
        public HitInstanceData hitData;
        public AudioDistances dists;
        public float lifetime;

    }
    [Server]
    public void init(float terrainRadius, float hitboxRadius, float halfHeight, UnitMovement m, HitInstanceData hitData, AudioDistances dists)
    {
        mover = m;
        Power p = mover.GetComponent<Power>();
        data = new ProjectileData
        {
            terrainRadius = terrainRadius,
            hitboxRadius = hitboxRadius,
            halfHeight = halfHeight,
            team = mover.GetComponent<TeamOwnership>().getTeam(),
            power = p.power,
            hitData = hitData,
            dists = dists,
            lifetime = BaseProjectileLifetime / p.scaleTime(),
        };
        setup(data);
    }

    void setup(ProjectileData data)
    {
        float terrainR = data.terrainRadius;
        float hitR = data.hitboxRadius;
        terrainHit.transform.localScale = new Vector3(terrainR, terrainR, terrainR) * 2;
        playerHit.transform.localScale = new Vector3(hitR, attackHitboxHalfHeight(HitType.Projectile, data.halfHeight, hitR) / 2, hitR) * 2;
        lifetime = data.lifetime;
        float speed = data.hitData.length / BaseProjectileLifetime;
        GetComponent<Rigidbody>().velocity = transform.forward * speed;

        setAudioDistances(Instantiate(FindObjectOfType<GlobalPrefab>().projectileAssetsPre[data.hitData.flair.visualIndex], visualScale.transform), data.dists);
        setThreatColor();
    }
    public void setThreatColor()
    {
        float threat = data.hitData.powerByStrength / FindObjectOfType<GlobalPlayer>().localPowerThreat;
        Color c = getIndicatorColor(data.team, threat, false).color;
        playerHit.GetComponent<ColorIndividual>().setColor(c);

    }

    public void onPlayerCollide(Collider other)
    {
        if (isServer && !collided.Contains(other))
        {
            hit(other.gameObject, mover, data.hitData, data.team, data.power, new KnockBackVectors { center = transform.position, direction = transform.forward });
            collided.Add(other);
        }

    }
    public void onTerrainCollide(Collider other)
    {
        Destroy(gameObject);
    }
}
