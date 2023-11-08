using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static AttackMachine;
using static GenerateAttack;
using static GenerateHit;
using static SpellSource;
using static Utils;

public class AttackSegment
{
    public List<AttackStageState> states;
    public WindState windup;
    public WindState winddown;
    public HitInstanceData hitData;
    public SpellSource sourcePoint;

    SourceLocation location;
    Optional<Vector3> targetOverride;

    bool pastWindup;
    AttackStageState currentState;
    AttackStageState indStopState;


    public AttackStageState enterSegment(UnitMovement mover)
    {
        pastWindup = false;
        if (mover.isServer)
        {

            sourcePoint = SpawnSource(mover, hitData, location, targetOverride);
        }
        return getNextState().state;
    }

    public void clientSyncSource(SpellSource s)
    {
        sourcePoint = s;
        constructIndicators();
    }

    public void sourceUpdate()
    {
        sourcePoint.OrderedUpdate();
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
        if (sourcePoint)
        {
            GameObject.Destroy(sourcePoint.gameObject);
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
    public static List<AttackSegment> buildStates(AttackInstanceData instance, UnitMovement mover, CastingLocationData castData)
    {


        List<AttackSegment> segments = new List<AttackSegment>();
        for (int i = 0; i < instance.segments.Length; i++)
        {
            SegmentInstanceData seg = instance.segments[i];
            AttackSegment finalSeg = new AttackSegment();
            List<AttackStageState> states = new List<AttackStageState>();

            WindState windup = new WindState(mover, seg.windup, false, castData.hardCast);


            ActionState hit = new ActionState(mover, finalSeg, seg.hit, seg.buff, seg.defense, castData.hardCast);
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
                        repeatStates.Add(new WindState(mover, seg.windRepeat, false, castData.hardCast));
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
            else if (seg.hit.type == HitType.Ground)
            {
                finalSeg.location = SourceLocation.WorldForward;
            }

            segments.Add(finalSeg);
        }

        return segments;
    }

    public enum SourceLocation : byte
    {
        World,
        WorldForward,
        Body,
        BodyFixed
    }
    static SpellSource SpawnSource(UnitMovement mover, HitInstanceData source, SourceLocation location, Optional<Vector3> targetOverride)
    {
        Size size = mover.GetComponentInChildren<Size>();
        Vector3 target = targetOverride.HasValue ? targetOverride.Value : mover.lookWorldPos;

        Transform body = mover.getSpawnBody().transform;
        uint team = mover.GetComponent<TeamOwnership>().getTeam();
        FloorNormal ground = mover.GetComponent<FloorNormal>();
        UnitEye eye = mover.GetComponentInChildren<UnitEye>();

        float range = source.type == HitType.Projectile ? 0 : source.range;

        MoveMode moveType = MoveMode.Parent;
        if (location == SourceLocation.World || location == SourceLocation.WorldForward)
        {
            moveType = MoveMode.World;
        }
        GameObject prefab = GameObject.FindObjectOfType<GlobalPrefab>().GroundTargetPre;
        Vector3 eyeForward = eye.transform.forward;
        Vector3 bodyFocus = body.position + eyeForward * size.scaledRadius;
        Vector3 targetDiff = target - bodyFocus;
        Vector3 flatDiff = targetDiff;
        flatDiff.y = 0;


        //float angleOff = Vector3.SignedAngle(body.forward, planarDiff, Vector3.up);
        //Vector3 forwardLine = Quaternion.AngleAxis(angleOff, Vector3.up) * diff;
        //Vector3 offset = forwardLine.normalized * Mathf.Min(length, Vector3.Dot(diff, forwardLine));
        GameObject instance;
        switch (location)
        {
            case SourceLocation.World:
            case SourceLocation.WorldForward:



                float distance = targetDiff.magnitude;
                if (source.type == HitType.Line)
                {
                    distance = Mathf.Max(0, distance - source.length / 2);
                }
                if (source.type == HitType.Projectile)
                {
                    distance = Mathf.Max(0, distance - source.range / 2);
                }

                Vector3 limitedDiff = targetDiff.normalized * Mathf.Clamp(distance, 0, range);
                if (location == SourceLocation.WorldForward)
                {
                    Vector3 forwardDiff = Mathf.Max(Vector3.Dot(targetDiff, eyeForward), 0) * eyeForward;
                    limitedDiff = forwardDiff.normalized * Mathf.Clamp(forwardDiff.magnitude, 0, range);
                    flatDiff = limitedDiff;
                    flatDiff.y = 0;
                }


                Quaternion faceRot = Quaternion.LookRotation(flatDiff);

                instance = GameObject.Instantiate(prefab, bodyFocus + limitedDiff, faceRot);
                instance.GetComponent<SpellSource>().offsetMult = 0;
                break;
            case SourceLocation.Body:
            case SourceLocation.BodyFixed:
            default:
                Transform targetTransform = location == SourceLocation.Body ? body : mover.transform;
                Quaternion rotation = location == SourceLocation.Body ? body.rotation : Quaternion.LookRotation(flatDiff);
                instance = GameObject.Instantiate(prefab, bodyFocus, rotation, targetTransform);
                ClientAdoption adopt = instance.GetComponent<ClientAdoption>();
                adopt.parent = mover.gameObject;
                adopt.useSubBody = location == SourceLocation.Body;
                break;

        }
        SpellSource instanceSource = instance.GetComponent<SpellSource>();

        instanceSource.init(size.sizeC, mover.gameObject, team, moveType);
        NetworkServer.Spawn(instance);


        return instanceSource;
    }
}





