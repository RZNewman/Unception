using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackUtils;
using static GenerateHit;
using Mirror;
using static IndicatorInstance;
using UnityEngine.VFX;

public class Projectile : NetworkBehaviour
{
    public GameObject playerHit;
    public GameObject terrainHit;
    public GameObject visualScale;

    public float speed;

    public static readonly float ProjectileLifetime = 1.5f;
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
        if (Time.time > birth + ProjectileLifetime)
        {
            Destroy(gameObject);
        }
    }
    struct ProjectileData
    {
        public float terrainRadius;
        public float playerRadius;
        public float halfHeight;
        public uint team;
        public float power;
        public HitInstanceData hitData;

    }
    [Server]
    public void init(float terrainRadius, float playerRadius, float halfHeight, UnitMovement m, HitInstanceData hitData)
    {
        mover = m;
        data = new ProjectileData
        {
            terrainRadius = terrainRadius,
            playerRadius = playerRadius,
            halfHeight = halfHeight,
            team = mover.GetComponent<TeamOwnership>().getTeam(),
            power = mover.GetComponent<Power>().power,
            hitData = hitData,
        };
        setup(data);
    }

    void setup(ProjectileData data)
    {
        float terrainR = data.terrainRadius;
        float playerR = data.playerRadius;
        terrainHit.transform.localScale = new Vector3(terrainR, terrainR, terrainR) * 2;
        playerHit.transform.localScale = new Vector3(playerR, Mathf.Max(data.halfHeight / 2, playerR / 2), playerR) * 2;
        float speed = data.hitData.length / ProjectileLifetime;
        GetComponent<Rigidbody>().velocity = transform.forward * speed;

        Instantiate(FindObjectOfType<GlobalPrefab>().projectileAssetsPre[data.hitData.flair.visualIndex], visualScale.transform);
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
