using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static GenerateHit;
using static UnitControl;
using static AttackUtils;
using static GenerateBuff;
using static SpellSource;
using static GenerateDefense;

public class ActionState : AttackStageState
{
    HitInstanceData attackData;

    BuffInstanceData buffData;

    DefenseInstanceData defData;

    AttackSegment segment;
    bool hardCast;

    public ActionState(UnitMovement m, AttackSegment seg, HitInstanceData data, BuffInstanceData dataB, DefenseInstanceData def, bool hardCasted) : base(m)
    {
        attackData = data;
        buffData = dataB;
        defData = def;
        segment = seg;
        hardCast = hardCasted;
    }
    public override void enter()
    {
        if (hardCast)
        {
            mover.GetComponent<AnimationController>().setAttack();
        }


        handleAttack();

    }

    void handleAttack()
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
                            center = sourcePoint.transform.position,
                            direction = sourcePoint.transform.forward
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
                float radius = GroundRadius(attackData.length, attackData.width);
                GroundParticle(sourcePoint.transform.position, radius, sourcePoint.aimRotation(AimType.Normal), attackData.flair, mover.sound.dists);
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

        if (defData != null)
        {
            SpawnBuff(mover.transform, BuffMode.Shield, defData.scales, defData.duration, defData.shield(mover.GetComponent<Power>().power), defData.regen(mover.GetComponent<Power>().power));
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
        return attackData;
    }
    public override void tick()
    {
        base.tick();

        Debug.LogError("Unreachable - Should transition");


    }

    public override StateTransition transition()
    {
        return new StateTransition(null, true);
    }



}
