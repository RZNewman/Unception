using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static GenerateHit;
using static UnitControl;
using static AttackUtils;
using static GenerateBuff;
using static SpellSource;
using static GenerateDefense;
using System.Linq;

public class ActionState : AttackStageState
{
    HitInstanceData attackData;

    BuffInstanceData buffData;

    DefenseInstanceData defData;

    AttackSegment segment;
    bool hardCast;
    bool useRangeForHit;

    public ActionState(UnitMovement m, AttackSegment seg, HitInstanceData data, BuffInstanceData dataB, DefenseInstanceData def, bool hardCasted, bool usesRangeForHit) : base(m)
    {
        attackData = data;
        buffData = dataB;
        defData = def;
        segment = seg;
        hardCast = hardCasted;
        useRangeForHit = usesRangeForHit;
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
        if (!mover.isServer)
        {
            return;
        }
        SpellSource[] sources = segment.sources;
        List<HitSource> hits = new List<HitSource>();
        List<GameObject> enemyHits = new List<GameObject>();
        ShapeData shapeData = getShapeData();
        HitList hitList = new HitList();

        foreach(SpellSource source in sources)
        {
            hits.AddRange(fireOneSource(source, shapeData, hitList));
        }

        foreach (HitSource hitSource in hits)
        {
            if (hit(hitSource.hit, mover, attackData,
                mover.GetComponent<TeamOwnership>().getTeam(),
                mover.GetComponent<Power>().power,
                new KnockBackVectors
                {
                    center = hitSource.source.transform.position,
                    direction = hitSource.source.transform.forward
                }, hitList))
            {
                enemyHits.Add(hitSource.hit);
            }

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
                foreach (GameObject h in enemyHits)
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

    struct HitSource
    {
        public GameObject hit;
        public SpellSource source;
    }

    List<HitSource> fireOneSource(SpellSource sourcePoint, ShapeData shapeData, HitList hitList)
    {
        List<GameObject> hits = new List<GameObject>();
        switch (attackData.type)
        {
            case HitType.Attached:
                //LineInfo info = LineCalculations(sourcePoint, attackData.range, attackData.length, attackData.width);
                ShapeParticle(sourcePoint, shapeData, attackData.shape, attackData.flair, mover.sound.dists);
                //LineParticle(info, attackData.flair, mover.sound.dists);
                //hits = LineAttack(info);
                hits = ShapeAttack(sourcePoint, shapeData);

                break;
            case HitType.ProjectileExploding:
                SpawnProjectile(sourcePoint, mover, attackData, buffData, hitList, mover.sound.dists);
                break;
            case HitType.GroundPlaced:
                //float radius = GroundRadius(attackData.length, attackData.width);
                ShapeParticle(sourcePoint, shapeData, attackData.shape, attackData.flair, mover.sound.dists);
                //GroundParticle(sourcePoint.transform.position, radius, sourcePoint.aimRotation(AimType.Normal), attackData.flair, mover.sound.dists);
                hits = ShapeAttack(sourcePoint, shapeData);
                //hits = GroundAttack(sourcePoint.transform.position, radius);
                break;

        }
        return hits.Select(h => new HitSource { hit = h, source = sourcePoint }).ToList();
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

    public ShapeData getShapeData()
    {
        return AttackUtils.getShapeData(attackData.shape, segment.capsuleSize, attackData.range, attackData.length, attackData.width, useRangeForHit);
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
