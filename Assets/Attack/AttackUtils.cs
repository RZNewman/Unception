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
using UnityEditor.SceneManagement;

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
                other.GetComponentInParent<EventManager>().fireHit(mover.gameObject);
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
        public HitInstanceData hitData;
        public SpellSource sourcePoint;

        bool pastWindup;
        AttackStageState currentState;
        AttackStageState indStopState;


        public AttackStageState enterSegment(UnitMovement mover)
        {
            pastWindup = false;
            if (mover.isServer)
            {

                sourcePoint = SpawnSource(mover, hitData);
            }
            return getNextState();
        }

        public void clientSyncSource(SpellSource s)
        {
            sourcePoint = s;
            constructIndicators();
        }

        public void IndicatorUpdate()
        {
            List<AttackStageState> indStates = new List<AttackStageState>();
            indStates.Add(currentState);
            indStates.AddRange(states);

            IndicatorOffsets offsets = new IndicatorOffsets
            {
                distance = Vector2.zero,
                time = 0,
            };
            int i = 0;
            while (i < indStates.Count && indStates[i] != indStopState)
            {

                AttackStageState state = indStates[i];
                if (state is ActionState)
                {
                    sourcePoint.updateIndicator(IndicatorType.Hit, offsets);
                }
                else if (state is DashState)
                {
                    sourcePoint.updateIndicator(IndicatorType.Dash, offsets);
                }

                offsets = offsets.sum(state.GetIndicatorOffsets());
                i++;
            }
        }

        public void exitSegment()
        {
            GameObject.Destroy(sourcePoint.gameObject);
        }

        public AttackStageState getNextState()
        {
            if (states.Count == 0)
            {
                return null;
            }
            if (currentState == windup)
            {
                pastWindup = true;
            }

            if (currentState is ActionState)
            {
                sourcePoint.killIndicatorType(IndicatorType.Hit);
            }
            else if (currentState is DashState)
            {
                sourcePoint.killIndicatorType(IndicatorType.Dash);
            }

            currentState = states[0];
            states.RemoveAt(0);
            if (currentState is WindState)
            {
                if (states.Count > 0 && sourcePoint)
                {
                    constructIndicators();
                }

            }

            return currentState;
        }

        void constructIndicators()
        {
            List<AttackStageState> indStates = new List<AttackStageState>();
            indStates.Add(currentState);
            indStates.AddRange(states);
            int windCount = 0;
            int i = 0;
            while (i < indStates.Count)
            {
                AttackStageState state = indStates[i];
                if (state is WindState)
                {
                    windCount++;
                    if (windCount > 1)
                    {
                        indStopState = state;
                        break;
                    }
                }

                if (state is ActionState)
                {
                    ActionState action = (ActionState)state;
                    HitInstanceData source = action.getSource();
                    if (source.type == HitType.Ground)
                    {
                        ((WindState)currentState).setGroundTarget(sourcePoint);
                    }
                    sourcePoint.buildHitIndicator(source);
                }
                else if (state is DashState)
                {
                    sourcePoint.buildDashIndicator(((DashState)state).getSource());
                }


                i++;
            }
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

    public static List<AttackSegment> buildStates(AttackInstanceData instance, UnitMovement mover)
    {


        List<AttackSegment> segments = new List<AttackSegment>();
        for (int i = 0; i < instance.segments.Length; i++)
        {
            SegmentInstanceData seg = instance.segments[i];
            AttackSegment finalSeg = new AttackSegment();
            List<AttackStageState> states = new List<AttackStageState>();

            WindState windup = new WindState(mover, seg.windup, false);
            WindState winddown = new WindState(mover, seg.winddown, true);

            ActionState hit = new ActionState(mover, seg.hit, seg.buff);
            finalSeg.hitData = seg.hit;

            states.Add(windup);
            List<AttackStageState> effectStates = new List<AttackStageState>();
            if (seg.repeat != null)
            {
                List<AttackStageState> repeatStates = new List<AttackStageState>();
                for (int j = 0; j < seg.repeat.repeatCount; j++)
                {
                    repeatStates.Add(hit);
                    if (seg.dash != null && seg.dashInside)
                    {
                        if (seg.dashAfter)
                        {
                            repeatStates.Add(new DashState(mover, seg.dash, true));
                        }
                        else
                        {
                            repeatStates.Insert(0, new DashState(mover, seg.dash, true));
                        }
                    }
                    if (j < seg.repeat.repeatCount - 1)
                    {
                        repeatStates.Add(new WindState(mover, seg.windRepeat, false));
                    }
                    effectStates.AddRange(repeatStates);
                    repeatStates.Clear();
                }
            }
            else
            {
                effectStates.Add(hit);
            }

            if (seg.dash != null && !seg.dashInside)
            {
                if (seg.dashAfter)
                {
                    effectStates.Add(new DashState(mover, seg.dash, true));
                }
                else
                {
                    effectStates.Insert(0, new DashState(mover, seg.dash, true));
                }
            }
            states.AddRange(effectStates);
            states.Add(winddown);
            finalSeg.states = states;
            finalSeg.windup = windup;
            finalSeg.winddown = winddown;
            segments.Add(finalSeg);
        }

        return segments;
    }

    public enum SourceLocation : byte
    {
        World,
        Body,
        BodyFixed
    }
    static SpellSource SpawnSource(UnitMovement mover, HitInstanceData source)
    {
        Size size = mover.GetComponentInChildren<Size>();
        Vector3 target = mover.lookWorldPos;

        Transform body = mover.getSpawnBody().transform;
        uint team = mover.GetComponent<TeamOwnership>().getTeam();
        FloorNormal ground = mover.GetComponent<FloorNormal>();

        float range = source.type == HitType.Projectile ? 0 : source.range;
        SourceLocation loc = SourceLocation.Body;
        MoveMode moveType = MoveMode.Parent;
        if (source.type == HitType.Ground)
        {
            loc = SourceLocation.World;
            moveType = MoveMode.World;
        }
        GameObject prefab = GameObject.FindObjectOfType<GlobalPrefab>().GroundTargetPre;
        Vector3 planarForward = ground.forwardPlanarWorld(body.forward);
        Vector3 bodyFocus = body.position + planarForward * size.scaledRadius;


        //float angleOff = Vector3.SignedAngle(body.forward, planarDiff, Vector3.up);
        //Vector3 forwardLine = Quaternion.AngleAxis(angleOff, Vector3.up) * diff;
        //Vector3 offset = forwardLine.normalized * Mathf.Min(length, Vector3.Dot(diff, forwardLine));
        GameObject instance;
        switch (loc)
        {
            case SourceLocation.World:

                Vector3 diff = target - bodyFocus;
                //Vector3 forwardDiff = Mathf.Max(Vector3.Dot(diff, body.forward), 0) * body.forward;
                //forwardDiff.y = diff.y;
                float distance = diff.magnitude;
                if (source.type == HitType.Line || source.type == HitType.Projectile)
                {
                    distance = Mathf.Max(0, distance - source.length / 2);
                }
                Vector3 limitedDiff = diff.normalized * Mathf.Clamp(distance, 0, range);
                //Vector3 forwardDiff = Mathf.Max(Vector3.Dot(diff, planarForward), 0) * planarForward;
                //forwardDiff.y = diff.y;
                //Vector3 limitedDiff = forwardDiff.normalized * Mathf.Clamp(forwardDiff.magnitude, 0, range);

                instance = GameObject.Instantiate(prefab, bodyFocus + limitedDiff, body.rotation);
                instance.GetComponent<SpellSource>().offsetMult = 0;
                break;
            case SourceLocation.Body:
            case SourceLocation.BodyFixed:
            default:
                Transform targetTransform = loc == SourceLocation.Body ? body : mover.transform;
                instance = GameObject.Instantiate(prefab, bodyFocus, body.rotation, targetTransform);
                ClientAdoption adopt = instance.GetComponent<ClientAdoption>();
                adopt.parent = mover.gameObject;
                adopt.useSubBody = loc == SourceLocation.Body;
                break;

        }
        SpellSource instanceSource = instance.GetComponent<SpellSource>();

        instanceSource.init(size.sizeC, mover.gameObject, team, moveType);
        //TODO Line attack flexible range
        NetworkServer.Spawn(instance);


        return instanceSource;
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
        Quaternion aim = floor.getAimRotation(source.transform.forward);
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
    public static LineInfo LineCalculations(SpellSource source, float range, float length, float width)
    {
        FloorNormal floor = source.GetComponent<FloorNormal>();
        Vector2 attackVec = new Vector2(length, width / 2);
        float maxDistance = attackVec.magnitude;
        Quaternion aim = floor.getAimRotation(source.transform.forward);
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
