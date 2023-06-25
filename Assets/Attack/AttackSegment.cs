using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static GenerateHit;
using static SpellSource;

public class AttackSegment
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
    public static List<AttackSegment> buildStates(AttackInstanceData instance, UnitMovement mover, bool hardCast)
    {


        List<AttackSegment> segments = new List<AttackSegment>();
        for (int i = 0; i < instance.segments.Length; i++)
        {
            SegmentInstanceData seg = instance.segments[i];
            AttackSegment finalSeg = new AttackSegment();
            List<AttackStageState> states = new List<AttackStageState>();

            WindState windup = new WindState(mover, seg.windup, false, hardCast);
            

            ActionState hit = new ActionState(mover, finalSeg, seg.hit, seg.buff, hardCast);
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
                        repeatStates.Add(new WindState(mover, seg.windRepeat, false, hardCast));
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
            if (seg.winddown.HasValue)
            {
                WindState winddown = new WindState(mover, seg.winddown.Value, true, hardCast);
                states.Add(winddown);
                finalSeg.winddown = winddown;
            }
           
            finalSeg.states = states;
            finalSeg.windup = windup;
            
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
}





