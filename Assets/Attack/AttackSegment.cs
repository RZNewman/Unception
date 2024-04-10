using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static AttackMachine;
using static AttackUtils;
using static GenerateAttack;
using static GenerateHit;
using static Size;
using static SpellSource;
using static Utils;

public class AttackSegment
{
    public List<AttackStageState> states;
    public WindState windup;
    public WindState winddown;
    public HitInstanceData hitData;
    SpellSource[] sourcePoints = new SpellSource[0];
    SpellSource primeSource;
    CapsuleSize sizeC;

    SourceLocation location;
    Optional<Vector3> targetOverride;

    bool pastWindup;
    AttackStageState currentState;
    AttackStageState indStopState;


    public AttackStageState enterSegment(UnitMovement mover)
    {
        pastWindup = false;
        sizeC = mover.GetComponentInChildren<Size>().sizeC;
        if (mover.isServer)
        {

            (sourcePoints, primeSource) = SpawnSources(mover, hitData, location, targetOverride);
        }
        return getNextState().state;
    }

    public CapsuleSize capsuleSize
    {
        get
        {
            return sizeC;
        }
    }

    public SpellSource[] sources
    {
        get
        {
            return sourcePoints;
        }
    }

    public void sourcePreUpdate()
    {
        foreach(SpellSource source in sourcePoints)
        {
            source.PreUpdate();
        }
        
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
            foreach(int id in state.indicatorIds)
            {
                foreach (SpellSource source in sourcePoints)
                {
                    source.updateIndicator(id, offsets, state.selfPercent);
                }
            }

            offsets = offsets.sum(state.GetIndicatorOffsets());
            i++;
        }
    }

    public void exitSegment()
    {
        if (sourcePoints.Length > 0)
        {
            foreach (SpellSource source in sourcePoints)
            {
                GameObject.Destroy(source.gameObject);
            }
            
        }

    }

    public struct StateTransitionInfo
    {
        public AttackStageState state;
        public bool triggeredCast;
    }

    public StateTransitionInfo getNextState()
    {
        bool triggeredCast = false;
        if (states.Count == 0)
        {
            return new StateTransitionInfo
            {
                state = null,
                triggeredCast = triggeredCast,
            };
        }
        if (currentState == windup)
        {
            pastWindup = true;
            triggeredCast = true;
        }

        if (currentState != null) {
            foreach (int id in currentState.indicatorIds)
            {
                foreach (SpellSource source in sourcePoints)
                {
                    source.killIndicator(id);
                }
            }
        }
        


        currentState = states[0];
        states.RemoveAt(0);
        if (currentState is WindState)
        {
            if (states.Count > 0 && sourcePoints.Length>0)
            {
                constructIndicators();
            }

        }

        return new StateTransitionInfo
        {
            state = currentState,
            triggeredCast = triggeredCast,
        };
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
            List<int> indIds = new List<int>();

            if (state is ActionState || (state is DashState && ((DashState)state).isHit))
            {

                HitInstanceData hitData;
                ShapeData shapeData;
                if(state is ActionState)
                {
                    ActionState action = (ActionState)state;
                    hitData = action.getSource();
                    shapeData = action.getShapeData();
                }
                else
                {
                    DashState dashState = (DashState)state;
                    hitData = dashState.getHit();
                    shapeData = dashState.getShapeData();
                }

                if (hitData.type == HitType.GroundPlaced)
                {
                    ((WindState)currentState).setGroundTarget(sourcePoints);
                }
                foreach (SpellSource source in sourcePoints)
                {
                    indIds.Add(source.buildHitIndicator(hitData, shapeData));
                }
                
            }
            
            if (state is DashState)
            {
                indIds.Add(primeSource.buildDashIndicator(((DashState)state).getSource()));
            }

            state.indicatorIds = indIds.ToArray();

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
    public static List<AttackSegment> buildStates(AttackInstanceData instance, UnitMovement mover, CastingLocationData castData)
    {


        List<AttackSegment> segments = new List<AttackSegment>();
        for (int i = 0; i < instance.segments.Length; i++)
        {
            SegmentInstanceData seg = instance.segments[i];
            AttackSegment finalSeg = new AttackSegment();
            List<AttackStageState> states = new List<AttackStageState>();

            WindState windup = new WindState(mover, seg.windup, false, castData.hardCast);


            AttackStageState getHitState()
            {
                return seg.hit.type switch
                {
                    HitType.DamageDash => new DashState(mover, finalSeg, seg.hit, seg.buff, seg.defense, true),
                    _ => new ActionState(mover, finalSeg, seg.hit, seg.buff, seg.defense, castData.hardCast, castData.usesRange(seg.hit.type)),
                };
            }
            finalSeg.hitData = seg.hit;

            states.Add(windup);
            List<AttackStageState> effectStates = new List<AttackStageState>();
            if (seg.repeat != null)
            {
                List<AttackStageState> repeatStates = new List<AttackStageState>();
                for (int j = 0; j < seg.repeat.repeatCount; j++)
                {
                    repeatStates.Add(getHitState());
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
                        repeatStates.Add(new WindState(mover, seg.windRepeat, false, castData.hardCast));
                    }
                    effectStates.AddRange(repeatStates);
                    repeatStates.Clear();
                }
            }
            else
            {
                effectStates.Add(getHitState());
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
            if (seg.winddown != null)
            {
                WindState winddown = new WindState(mover, seg.winddown, true, castData.hardCast);
                states.Add(winddown);
                finalSeg.winddown = winddown;
            }

            finalSeg.states = states;
            finalSeg.windup = windup;

            finalSeg.location = SourceLocation.Body;
            finalSeg.targetOverride = new Optional<Vector3>();
            if (!castData.hardCast)
            {
                finalSeg.location = castData.locationOverride;
                finalSeg.targetOverride = new Optional<Vector3>(castData.triggeredPosition);
            }
            else if (seg.hit.type == HitType.GroundPlaced)
            {
                finalSeg.location = SourceLocation.WorldForward;
            }

            segments.Add(finalSeg);
        }

        return segments;
    }

    public enum SourceLocation : byte
    {
        World=1,
        WorldForward,
        Body,
        BodyFixed
    }
    static (SpellSource[], SpellSource) SpawnSources(UnitMovement mover, HitInstanceData hitData, SourceLocation location, Optional<Vector3> targetOverride)
    {
        SpellSource prime;
        List<SpellSource> instances = new List<SpellSource>();

        Size size = mover.GetComponentInChildren<Size>();
        Vector3 target = targetOverride.HasValue ? targetOverride.Value : mover.lookWorldPos;

        Transform body = mover.getSpawnBody().transform;
        uint team = mover.GetComponent<TeamOwnership>().getTeam();
        FloorNormal ground = mover.GetComponent<FloorNormal>();
        UnitEye eye = mover.GetComponentInChildren<UnitEye>();

        float range = hitData.type == HitType.ProjectileExploding || hitData.type == HitType.ProjectileWave ? 0 : hitData.range;

        MoveMode moveType = MoveMode.Parent;
        if (location == SourceLocation.World || location == SourceLocation.WorldForward)
        {
            moveType = MoveMode.World;
        }
        GameObject prefab = GlobalPrefab.gPre.GroundTargetPre;
        Vector3 eyeForward = eye.transform.forward;
        Vector3 bodyFocus = body.position + eyeForward * size.scaledRadius;
        Vector3 targetDiff = target - bodyFocus;
        Vector3 flatDiff = targetDiff;
        flatDiff.y = 0;


        //float angleOff = Vector3.SignedAngle(body.forward, planarDiff, Vector3.up);
        //Vector3 forwardLine = Quaternion.AngleAxis(angleOff, Vector3.up) * diff;
        //Vector3 offset = forwardLine.normalized * Mathf.Min(length, Vector3.Dot(diff, forwardLine));
        GameObject instance;
        Quaternion additionalRot;
        switch (location)
        {
            case SourceLocation.World:
            case SourceLocation.WorldForward:



                float distance = targetDiff.magnitude;
                //if (source.type == HitType.Line)
                //{
                //    distance = Mathf.Max(0, distance - source.length / 2);
                //}
                //if (source.type == HitType.Projectile)
                //{
                //    distance = Mathf.Max(0, distance - source.range / 2);
                //}

                Vector3 limitedDiff = targetDiff.normalized * Mathf.Clamp(distance, 0, range);
                if (location == SourceLocation.WorldForward)
                {
                    Vector3 forwardDiff = Mathf.Max(Vector3.Dot(targetDiff, eyeForward), 0) * eyeForward;
                    limitedDiff = forwardDiff.normalized * Mathf.Clamp(forwardDiff.magnitude, 0, range);
                    flatDiff = limitedDiff;
                    flatDiff.y = 0;
                }

                Vector3 primePos = bodyFocus + limitedDiff;
                for (int i = 0; i < hitData.multiple; i++)
                {
                    Quaternion faceRot = flatDiff.magnitude > 0 ? Quaternion.LookRotation(flatDiff) : Quaternion.LookRotation(body.forward);
                    additionalRot = Quaternion.AngleAxis((i - (hitData.multiple / 2)) * hitData.multipleArcSpacing, faceRot * Vector3.up);

                    Vector3 instancePos = bodyFocus + additionalRot * limitedDiff;
                    instance = GameObject.Instantiate(prefab, instancePos, additionalRot * faceRot);
                    SpellSource source = instance.GetComponent<SpellSource>();
                    source.movementOffset = instancePos - primePos;
                    source.multipleRotation = additionalRot;
                    source.offsetMult = 0;
                    instances.Add(source);
                }
                break;
            case SourceLocation.Body:
            case SourceLocation.BodyFixed:
            default:
                Transform targetTransform = location == SourceLocation.Body ? body : mover.transform;
                for(int i = 0; i <hitData.multiple; i++)
                {
                    Quaternion primeRotation = location == SourceLocation.Body ? body.rotation : mover.AimRotation(flatDiff);
                    additionalRot = Quaternion.AngleAxis((i - (hitData.multiple / 2)) * hitData.multipleArcSpacing, primeRotation * Vector3.up);

                    instance = GameObject.Instantiate(prefab, targetTransform.position, additionalRot *primeRotation, targetTransform);
                    ClientAdoption adopt = instance.GetComponent<ClientAdoption>();
                    adopt.parent = mover.gameObject;
                    adopt.useSubBody = location == SourceLocation.Body;
                    SpellSource source = instance.GetComponent<SpellSource>();
                    source.multipleRotation = additionalRot;
                    instances.Add(source);
                }
                
                break;

        }
        foreach(SpellSource source in instances)
        {
            source.init(size.sizeC, mover.gameObject, team, moveType);
            NetworkServer.Spawn(source.gameObject);
        }

        prime = instances[hitData.multiple / 2];
        return (instances.ToArray(), prime);
    }
}





