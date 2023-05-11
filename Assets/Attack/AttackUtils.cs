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

            if (mover)
            {
                other.GetComponentInParent<LifeManager>().getHit(mover.gameObject);
            }
            Health h = other.GetComponentInParent<Health>();
            Posture p = other.GetComponentInParent<Posture>();
            if (h)
            {
                float damage = hitData.damage(power);
                if (p && p.isStunned)
                {
                    damage *= 1.1f;
                }
                h.takeDamage(damage);
            }

            if (p)
            {
                p.takeStagger(hitData.stagger);
            }
            UnitMovement otherMover = other.GetComponentInParent<UnitMovement>();
            if (otherMover)
            {
                Vector3 knockBackVec;
                switch (hitData.knockBackType)
                {
                    case KnockBackType.inDirection:
                        knockBackVec = hitData.knockback * knockbackData.direction;

                        break;
                    case KnockBackType.fromCenter:
                        Vector3 dir = other.transform.position - knockbackData.center;
                        dir.y = 0;
                        dir.Normalize();
                        knockBackVec = hitData.knockback * dir;
                        break;
                    default:
                        throw new System.Exception("No kb type");
                }
                switch (hitData.knockBackDirection)
                {
                    case KnockBackDirection.Backward:
                        knockBackVec *= -1;
                        break;
                }
                otherMover.applyForce(knockBackVec);
                otherMover.applyForce(hitData.knockup * Vector3.up);
            }

            return true;
        }
        return false;
    }
    public struct AttackSegment
    {
        public List<AttackStageState> states;
        public WindState windup;
        public WindState winddown;
        public GameObject groundTargetInstance;

        bool pastWindup;
        AttackStageState currentState;

        public AttackStageState enterSegment(UnitMovement mover, Cast cast)
        {
            pastWindup = false;
            return nextState(mover, cast);
        }
        public void exitSegment()
        {
            cleanInstances();
        }

        void cleanInstances()
        {
            if (groundTargetInstance)
            {
                GameObject.Destroy(groundTargetInstance);
            }
        }
        public AttackStageState getNextState(UnitMovement mover, Cast cast)
        {
            cast.nextStage();
            return nextState(mover, cast);
        }

        AttackStageState nextState(UnitMovement mover, Cast cast)
        {
            if (states.Count == 0)
            {
                return null;
            }
            if (currentState == windup)
            {
                pastWindup = true;
            }
            currentState = states[0];

            if (currentState is WindState && states.Count > 1)
            {
                List<AttackStageState> indicatorBuild = new List<AttackStageState>();
                indicatorBuild.Add(states[0]);
                int i = 1;
                while (i < states.Count && !(states[i] is WindState))
                {
                    AttackStageState state = states[i];
                    indicatorBuild.Add(state);
                    if (state is ActionState)
                    {
                        ActionState action = (ActionState)state;
                        HitInstanceData source = action.getSource();
                        if (source.type == HitType.Ground)
                        {
                            GameObject body = mover.getSpawnBody();
                            Size s = body.GetComponentInChildren<Size>();
                            if (groundTargetInstance == null)
                            { 
                                //TODO Two ground target options, how to sync up?
                                groundTargetInstance = SpawnGroundTarget(body.transform, s.scaledRadius, s.scaledHalfHeight, mover.lookWorldPos, source.range, source.length, mover.isServer);
                            }
                            
                            groundTargetInstance.GetComponent<GroundTarget>().height = s.indicatorHeight;
                            ((WindState)currentState).setGroundTarget(groundTargetInstance, new FloorNormal.GroundSearchParams
                            {
                                radius = 0.2f,
                                distance = s.scaledHalfHeight * 1.1f,
                            });
                            action.setGroundTarget(groundTargetInstance);
                        }
                    }

                    i++;
                }
                cast.buildIndicator(indicatorBuild, this);
            }






            states.RemoveAt(0);
            return currentState;
        }

        public float remainingWindDown()
        {
            if (pastWindup)
            {
                return winddown.remainingDuration;
            }
            else
            {
                return 0;
            }
        }
        public bool inWindup()
        {
            return currentState == windup;
        }
    }


    static GameObject SpawnGroundTarget(Transform body, float radius, float height, Vector3 target, float range, float length, bool isServer)
    {
        GameObject prefab = GameObject.FindObjectOfType<GlobalPrefab>().GroundTargetPre;
        Vector3 bodyFocus = body.position + body.forward * radius;
        Vector3 diff = target - bodyFocus;


        Vector3 forwardDiff = Mathf.Max(Vector3.Dot(diff, body.forward), 0) * body.forward;
        forwardDiff.y = diff.y;
        Vector3 limitedDiff = forwardDiff.normalized * Mathf.Clamp(forwardDiff.magnitude, range, range + length);

        //float angleOff = Vector3.SignedAngle(body.forward, planarDiff, Vector3.up);
        //Vector3 forwardLine = Quaternion.AngleAxis(angleOff, Vector3.up) * diff;
        //Vector3 offset = forwardLine.normalized * Mathf.Min(length, Vector3.Dot(diff, forwardLine));


        GameObject instance = GameObject.Instantiate(prefab, bodyFocus + limitedDiff, body.rotation);
        if (isServer)
        {
            NetworkServer.Spawn(instance);
        }

        return instance;
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

    public static void SpawnProjectile(FloorNormal floor, Transform body, float radius, float halfHeight, UnitMovement mover, HitInstanceData hitData, BuffInstanceData buffData, AudioDistances dists)
    {
        GameObject prefab = GameObject.FindObjectOfType<GlobalPrefab>().ProjectilePre;
        Vector3 groundFocus = body.position + body.forward * radius + Vector3.down * halfHeight;
        Vector3 bodyFocus = groundFocus + floor.normal * halfHeight;
        Quaternion aim = floor.getAimRotation(body.forward);
        GameObject instance = GameObject.Instantiate(prefab, bodyFocus, aim);
        Projectile p = instance.GetComponent<Projectile>();
        float hitRadius = hitData.width / 2;
        float terrainRadius = Mathf.Min(hitRadius, halfHeight * 0.5f);
        p.init(terrainRadius, hitRadius, halfHeight, mover, hitData, buffData, dists);
        NetworkServer.Spawn(instance);
    }

    public static void SpawnBuff(BuffInstanceData buff, Transform target)
    {
        GameObject prefab = GameObject.FindObjectOfType<GlobalPrefab>().BuffPre;
        GameObject instance = GameObject.Instantiate(prefab, target);
        instance.GetComponent<ClientAdoption>().parent = target.gameObject;
        instance.GetComponent<Buff>().setup(buff.durration, Power.scaleTime(buff.powerAtGen));
        instance.GetComponent<StatHandler>().setStats(buff.stats);
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
    public static LineInfo LineCalculations(FloorNormal floor, Transform body, float radius, float halfHeight, float range, float length, float width)
    {

        Vector3 groundFocus = body.position + body.forward * radius + Vector3.down * halfHeight;
        Vector3 bodyFocus = groundFocus + floor.normal * halfHeight;
        Vector2 attackVec = new Vector2(length, width / 2);
        float maxDistance = attackVec.magnitude;
        Quaternion aim = floor.getAimRotation(body.forward);
        Vector3 attackFocus = bodyFocus + aim * Vector3.forward * range;
        Vector3 boxCenter = attackFocus + maxDistance * 0.5f * (aim * Vector3.forward);
        float boxHeight = attackHitboxHalfHeight(HitType.Line, halfHeight, maxDistance);
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
            bodyForward = body.forward,
            occlusionOrigin = bodyFocus,
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
