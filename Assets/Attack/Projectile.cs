using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackUtils;
using static GenerateHit;
using Mirror;
using static IndicatorInstance;

public class Projectile : NetworkBehaviour
{
    public GameObject playerHit;
    public GameObject terrainHit;

    public float speed;

    public static readonly float ProjectileLifetime = 1.5f;
    float birth;

    UnitMovement mover;
    uint team;
    float powerSnapshot;

    [SyncVar]
    HitInstanceData hitInstance;
    List<Collider> collided = new List<Collider>();
    private void Start()
    {
        birth = Time.time;
    }
    private void FixedUpdate()
    {
        if (Time.time > birth + ProjectileLifetime)
        {
            Destroy(gameObject);
        }
    }

    public void init(float terrainRadius, float playerRadius, float halfHeight, UnitMovement m, HitInstanceData hitData)
    {
        mover = m;
        team = mover.GetComponent<TeamOwnership>().getTeam();
        powerSnapshot = mover.GetComponent<Power>().power;
        terrainHit.transform.localScale = new Vector3(terrainRadius, terrainRadius, terrainRadius) * 2;
        playerHit.transform.localScale = new Vector3(playerRadius, Mathf.Max(halfHeight / 2, playerRadius / 2), playerRadius) * 2;
        float speed = hitData.length / ProjectileLifetime;
        GetComponent<Rigidbody>().velocity = transform.forward * speed;

        hitInstance = hitData;
        setThreatColor();
    }
    public void setThreatColor()
    {
        float threat = hitInstance.relativePower / FindObjectOfType<GlobalPlayer>().localPower;
        Color c = getIndicatorColor(mover.GetComponent<TeamOwnership>().getTeam(), threat);
        playerHit.GetComponent<ColorIndividual>().setColor(c);
        c.a = Mathf.Clamp01(c.a + 0.2f);
        terrainHit.GetComponent<ColorIndividual>().setColor(c);

    }

    public void onPlayerCollide(Collider other)
    {
        if (!collided.Contains(other))
        {
            hit(other.gameObject, mover, hitInstance, team, powerSnapshot, new KnockBackVectors { center = transform.position, direction = transform.forward });
            collided.Add(other);
        }

    }
    public void onTerrainCollide(Collider other)
    {
        Destroy(gameObject);
    }
}
