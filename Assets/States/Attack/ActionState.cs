using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static GenerateHit;
using static UnitControl;
using static AttackUtils;
using static GenerateBuff;
using static SpellSource;

public class ActionState : AttackStageState
{
    HitInstanceData actionData;

    BuffInstanceData buffData;

    AttackSegment segment;

    public ActionState(UnitMovement m, AttackSegment seg, HitInstanceData data, BuffInstanceData dataB) : base(m)
    {
        actionData = data;
        buffData = dataB;
        segment = seg;
    }
    public override void enter()
    {
        mover.GetComponent<AnimationController>().setAttack();

        handleAttack(actionData);

    }

    void handleAttack(HitInstanceData attackData)
    {
        SpellSource sourcePoint = segment.sourcePoint;
        List<GameObject> hits = new List<GameObject>();
        switch (attackData.type)
        {
            case HitType.Line:
                LineInfo info = LineCalculations(sourcePoint, attackData.range, attackData.length, attackData.width);
                LineParticle(info, attackData.flair, mover.sound.dists);
                if (!mover.isServer)
                {
                    return;
                }
                hits = LineAttack(info);
                foreach (GameObject o in hits)
                {
                    hit(o, mover, attackData,
                        mover.GetComponent<TeamOwnership>().getTeam(),
                        mover.GetComponent<Power>().power,
                        new KnockBackVectors
                        {
                            center = mover.transform.position,
                            direction = mover.getSpawnBody().transform.forward
                        });

                }
                break;
            case HitType.Projectile:
                if (!mover.isServer)
                {
                    return;
                }
                SpawnProjectile(sourcePoint, mover, attackData, buffData, mover.sound.dists);
                break;
            case HitType.Ground:
                float radius = (attackData.width + attackData.length) / 2;
                Quaternion aim = sourcePoint.GetComponent<FloorNormal>().getAimRotation(sourcePoint.transform.forward);
                GroundParticle(sourcePoint.transform.position, radius, aim, attackData.flair, mover.sound.dists);
                if (!mover.isServer)
                {
                    return;
                }
                hits = GroundAttack(sourcePoint.transform.position, radius);
                foreach (GameObject o in hits)
                {
                    hit(o, mover, attackData,
                        mover.GetComponent<TeamOwnership>().getTeam(),
                        mover.GetComponent<Power>().power,
                        new KnockBackVectors
                        {
                            center = sourcePoint.transform.position,
                            direction = sourcePoint.transform.forward
                        });

                }
                break;

        }

        if (buffData != null)
        {
            if (!mover.isServer)
            {
                return;
            }
            if (buffData.type == BuffType.Buff)
            {
                SpawnBuff(buffData, mover.transform);
            }
            else
            {
                foreach (GameObject h in hits)
                {
                    BuffManager bm = h.GetComponentInParent<BuffManager>();
                    if (bm)
                    {
                        SpawnBuff(buffData, bm.transform);
                    }


                }
            }

        }
    }

    public override IndicatorOffsets GetIndicatorOffsets()
    {
        return new IndicatorOffsets
        {
            distance = Vector3.zero,
            time = 0,
        };
    }

    public HitInstanceData getSource()
    {
        return actionData;
    }
    public override void tick()
    {
        base.tick();

        Debug.LogError("Unreachable - Should transition");
        //UnitInput inp = mover.input;


        //mover.rotate(inp, false, lookMultiplier);
        //mover.move(inp, moveMultiplier);


    }

    public override StateTransition transition()
    {
        return new StateTransition(null, true);
    }



}
