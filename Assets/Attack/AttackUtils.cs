using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static AiHandler;
using UnityEngine.UIElements;
using static GenerateHit;
using static UnitControl;
using static UnitSound;
using static Utils;
using static GenerateBuff;
using Unity.Burst.Intrinsics;
using static GenerateAttack;
using System;
using static SpellSource;
using static GenerateHit.HitInstanceData;
using static EventManager;

public static class AttackUtils
{
    public struct KnockBackVectors
    {
        public Vector3 center;
        public Vector3 direction;
    }
    public static bool hit(GameObject other, UnitMovement mover, HitInstanceData hitData, uint team, float power, KnockBackVectors knockbackData)
    {
        if (other.GetComponentInParent<TeamOwnership>().getTeam() != team)
        {
            UnitMovement otherMover = other.GetComponentInParent<UnitMovement>();
            DamageValues damage = hitData.damage(power, otherMover && otherMover.isIncapacitated);
            if (mover)
            {

                other.GetComponentInParent<EventManager>().fireHit(new GetHitEventData
                {
                    other = mover.gameObject,
                    powerByStrength = hitData.powerByStrength,
                    damage = damage.instant,
                });
            }

            Health h = other.GetComponentInParent<Health>();
            Posture p = other.GetComponentInParent<Posture>();
            Mezmerize mez = other.GetComponentInParent<Mezmerize>();
            Knockdown kDown = other.GetComponentInParent<Knockdown>();
            if (h)
            {

                if (damage.dot > 0)
                {
                    SpawnBuff(otherMover.transform, BuffMode.Dot, hitData.powerAtGen, damage.dotTime, damage.dot);
                }
                h.takeDamageHit(damage.instant);
                if (damage.expose > 0)
                {
                    SpawnBuff(otherMover.transform, BuffMode.Expose, hitData.powerAtGen, 10f / mover.GetComponent<Power>().scaleTime(), damage.expose);
                }
            }

            if (p)
            {
                float stagger = hitData.stagger;
                if (mez && mez.isMezmerized)
                {
                    stagger *= 1.1f;
                }
                p.takeStagger(stagger);
            }
            if (mez)
            {
                float focusHit = hitData.mezmerize;
                if (kDown && kDown.knockedDown)
                {
                    focusHit *= 1.1f;
                }
                mez.takeFocus(focusHit);
            }


            if (otherMover)
            {
                Vector3 knockBackDir;
                switch (hitData.knockBackType)
                {
                    case KnockBackType.inDirection:
                        knockBackDir = knockbackData.direction;
                        break;
                    case KnockBackType.fromCenter:
                        Vector3 dir = other.transform.position - knockbackData.center;
                        dir.y = 0;
                        dir.Normalize();
                        knockBackDir = dir;
                        break;
                    default:
                        throw new System.Exception("No kb type");
                }
                switch (hitData.knockBackDirection)
                {
                    case KnockBackDirection.Backward:
                        knockBackDir *= -1;
                        break;
                }
                otherMover.knock(knockBackDir, hitData.knockback, hitData.knockup);
            }

            return true;
        }
        return false;
    }


    public static float attackHitboxHalfHeight(HitType type, float halfUnitHeight, float attackDistance)
    {
        switch (type)
        {
            case HitType.Line:
                return Mathf.Max(halfUnitHeight * 1.5f, attackDistance);
            case HitType.Projectile:
                return Mathf.Max(halfUnitHeight, attackDistance);
            case HitType.Ground:
                return attackDistance;
            default:
                return halfUnitHeight * 2;
        }
    }

    public static void SpawnProjectile(SpellSource source, UnitMovement mover, HitInstanceData hitData, BuffInstanceData buffData, AudioDistances dists)
    {
        FloorNormal floor = source.GetComponent<FloorNormal>();
        GameObject prefab = GameObject.FindObjectOfType<GlobalPrefab>().ProjectilePre;
        Quaternion aim = source.aimRotation(AimType.Normal);
        GameObject instance = GameObject.Instantiate(prefab, source.transform.position, aim);
        Projectile p = instance.GetComponent<Projectile>();
        float hitRadius = hitData.width / 2;
        float terrainRadius = Mathf.Min(hitRadius, source.sizeCapsule.distance * 0.5f);
        p.init(terrainRadius, hitRadius, source.sizeCapsule.distance, mover, hitData, buffData, dists);
        NetworkServer.Spawn(instance);
    }

    public static void SpawnBuff(BuffInstanceData buff, Transform target)
    {
        GameObject prefab = GameObject.FindObjectOfType<GlobalPrefab>().BuffPre;
        if (buff.slot.HasValue)
        {
            target = target.GetComponent<AbiltyManager>().getAbility(buff.slot.Value).transform;
        }
        GameObject instance = GameObject.Instantiate(prefab, target);
        instance.GetComponent<ClientAdoption>().parent = target.gameObject;
        instance.GetComponent<Buff>().setup(buff);
        instance.GetComponent<StatHandler>().setStats(buff.stats);
        NetworkServer.Spawn(instance);
    }

    public static void SpawnBuff(Transform target, BuffMode buffMode, float powerAtGen, float duration, float value, float regen = 0)
    {
        GameObject prefab = GameObject.FindObjectOfType<GlobalPrefab>().BuffPre;
        GameObject instance = GameObject.Instantiate(prefab, target);
        instance.GetComponent<ClientAdoption>().parent = target.gameObject;
        instance.GetComponent<Buff>().setup(buffMode, powerAtGen, duration, value, regen);
        NetworkServer.Spawn(instance);
    }

    public struct LineInfo
    {
        public Vector3 boxCenter;
        public Vector3 boxHalfs;
        public Vector3 capsuleStart;
        public Vector3 capsuleEnd;
        public Vector3 occlusionOrigin;
        public Quaternion aim;
        public float maxDistance;
        public Vector3 bodyForward;
    }
    public static LineInfo LineCalculations(SpellSource source, float range, float length, float width)
    {      
        Vector2 attackVec = new Vector2(length, width / 2);
        float maxDistance = attackVec.magnitude;
        FloorNormal floor = source.GetComponent<FloorNormal>();
        Quaternion aim = source.aimRotation(AimType.Normal);
        Vector3 attackFocus = source.transform.position + aim * Vector3.forward * range;
        Vector3 boxCenter = attackFocus + maxDistance * 0.5f * (aim * Vector3.forward);
        float boxHeight = attackHitboxHalfHeight(HitType.Line, source.sizeCapsule.distance, maxDistance);
        Vector3 boxHalfs = new Vector3(width / 2, boxHeight / 2, maxDistance / 2);

        float capsuleHeightFactor = Mathf.Max(boxHeight / 2 - maxDistance, 0);
        Vector3 capsuleHeightDiff = floor.normal * capsuleHeightFactor;
        Vector3 capsuleStart = attackFocus + capsuleHeightDiff;
        Vector3 capsuleEnd = attackFocus - capsuleHeightDiff;
        return new LineInfo
        {
            boxCenter = boxCenter,
            boxHalfs = boxHalfs,
            capsuleEnd = capsuleEnd,
            capsuleStart = capsuleStart,
            aim = aim,
            maxDistance = maxDistance,
            bodyForward = source.transform.forward,
            occlusionOrigin = source.transform.position,
        };
    }

    public static List<GameObject> LineAttack(LineInfo info)
    {
        List<GameObject> hits = new List<GameObject>();
        List<GameObject> tempHits = new List<GameObject>();

        RaycastHit[] boxHits = Physics.BoxCastAll(info.boxCenter, info.boxHalfs, info.bodyForward, info.aim, 0.0f, LayerMask.GetMask("Players", "Breakables"));
        //RaycastHit[] sphereHits = Physics.SphereCastAll(bodyFocus, maxDistance, body.forward, 0.0f, LayerMask.GetMask("Players"));

        RaycastHit[] capsuleHits = Physics.CapsuleCastAll(info.capsuleStart, info.capsuleEnd, info.maxDistance, info.bodyForward, 0.0f, LayerMask.GetMask("Players", "Breakables"));

        //Debug.DrawLine(bodyFocus, bodyFocus + body.forward * maxDistance, Color.blue, 3.0f); ;
        //Debug.DrawLine(bodyFocus, bodyFocus + (body.forward+body.up).normalized * maxDistance, Color.blue, 3.0f);
        //DrawBox(boxCenter, aim, boxHalfs * 2, Color.blue);
        //Debug.DrawLine(capsuleStart, capsuleEnd, Color.red);
        //Debug.DrawLine(capsuleStart, capsuleStart+ body.forward*maxDistance, Color.red);
        //Debug.DrawLine(capsuleEnd, capsuleEnd + body.forward * maxDistance, Color.red);
        //Debug.Break();

        foreach (RaycastHit hit in boxHits)
        {
            GameObject obj = hit.collider.gameObject;
            tempHits.Add(obj);
        }
        foreach (RaycastHit hit in capsuleHits)
        {
            GameObject obj = hit.collider.gameObject;
            Vector3 lineDiff = hit.collider.bounds.center - info.occlusionOrigin;
            if (tempHits.Contains(obj)
                && !Physics.Raycast(info.occlusionOrigin, lineDiff, lineDiff.magnitude, LayerMask.GetMask("Terrain")))
            {

                hits.Add(obj);
            }
        }

        return hits;

    }

    public static void LineParticle(LineInfo info, HitFlair flair, AudioDistances dists)
    {
        GlobalPrefab gp = GameObject.FindObjectOfType<GlobalPrefab>();
        GameObject prefab = gp.ParticlePre;
        GameObject i = GameObject.Instantiate(prefab, info.boxCenter, info.aim);
        i.transform.localScale = info.boxHalfs * 2;
        i.GetComponent<Particle>().setVisualsLine(gp.lineAssetsPre[flair.visualIndex], dists);

    }

    public static float GroundRadius(float length, float width)
    {
        return (length + width) / 2;
    }
    public static List<GameObject> GroundAttack(Vector3 origin, float radius)
    {
        List<GameObject> hits = new List<GameObject>();

        RaycastHit[] sphereHits = Physics.SphereCastAll(origin, radius, Vector3.forward, 0.0f, LayerMask.GetMask("Players", "Breakables"));



        foreach (RaycastHit hit in sphereHits)
        {
            GameObject obj = hit.collider.gameObject;
            hits.Add(obj);
        }


        return hits;

    }

    public static void GroundParticle(Vector3 origin, float radius, Quaternion aim, HitFlair flair, AudioDistances dists)
    {
        GlobalPrefab gp = GameObject.FindObjectOfType<GlobalPrefab>();
        GameObject prefab = gp.ParticlePre;
        GameObject i = GameObject.Instantiate(prefab, origin, aim);
        i.transform.localScale = Vector3.one * radius * 2;
        i.GetComponent<Particle>().setVisualsCircle(gp.groundAssetsPre[flair.visualIndex], dists);


    }
}
