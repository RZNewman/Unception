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
using System;
using static GenerateHit.HitInstanceData;
using static Persistent;

public class ActionState : AttackStageState
{
    HitInstanceData attackData;

    BuffInstanceData buffData;

    DefenseInstanceData defData;

    AttackSegment segment;
    bool hardCast;
    RangeForShape useRangeForHit;

    public ActionState(UnitMovement m, AttackSegment seg, HitInstanceData data, BuffInstanceData dataB, DefenseInstanceData def, bool hardCasted, RangeForShape usesRangeForHit) : base(m)
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
        ShapeData shapeData = getShapeData();
        HitList hitList = new HitList();

        foreach(SpellSource source in sources)
        {
            List<GameObject> hits = fireOneSource(source, shapeData, hitList);
            float power = mover.GetComponent<Power>().power;
            HarmPortions harm = attackData.getHarmValues(power, new KnockBackVectors
            {
                center = source.transform.position,
                direction = source.transform.forward
            });
            if(attackData.willCreateAura)
            {

                PersistMode mode = attackData.dotType switch
                {
                    DotType.Placed => PersistMode.AuraPlaced,
                    DotType.Channeled => PersistMode.AuraChanneled,
                    DotType.Carried => PersistMode.AuraCarried,
                    _ => throw new NotImplementedException(),
                };
                harm.OverTime = null;
                SpawnPersistent(source, mover,attackData, null, null, mover.sound.dists, mode);
            }


            foreach (GameObject other in hits)
            {
                
                hit(other, mover,
                    harm,
                    mover.GetComponent<TeamOwnership>().getTeam(),
                    hitList, buffData);


            }
        }

        

        if (buffData != null)
        {
            if (buffData.type == BuffType.Buff)
            {
                SpawnBuff(buffData, mover.transform);
            }

        }

        if (defData != null)
        {
            SpawnShield(mover.transform, defData.scales, defData.duration, defData.shield(mover.GetComponent<Power>().power), defData.regen(mover.GetComponent<Power>().power));
        }
    }

    struct HitSource
    {
        public GameObject hit;
        public SpellSource source;
    }

    List<GameObject> fireOneSource(SpellSource sourcePoint, ShapeData shapeData, HitList hitList)
    {
        List<GameObject> hits = new List<GameObject>();
        switch (attackData.type)
        {
            case HitType.Attached:
                //LineInfo info = LineCalculations(sourcePoint, attackData.range, attackData.length, attackData.width);
                if(!attackData.willCreateAura)
                {
                    ShapeParticle(sourcePoint, shapeData, attackData.shape, attackData.flair, mover.sound.dists);
                }
                //LineParticle(info, attackData.flair, mover.sound.dists);
                //hits = LineAttack(info);
                hits = ShapeAttack(sourcePoint, shapeData);

                break;
            case HitType.ProjectileExploding:
            case HitType.ProjectileWave:
                SpawnPersistent(sourcePoint, mover, attackData, buffData, hitList, mover.sound.dists, Persistent.PersistMode.Default);
                break;
            case HitType.GroundPlaced:
                //float radius = GroundRadius(attackData.length, attackData.width);
                if (!attackData.willCreateAura)
                {
                    ShapeParticle(sourcePoint, shapeData, attackData.shape, attackData.flair, mover.sound.dists);
                }
                //GroundParticle(sourcePoint.transform.position, radius, sourcePoint.aimRotation(AimType.Normal), attackData.flair, mover.sound.dists);
                hits = ShapeAttack(sourcePoint, shapeData);
                //hits = GroundAttack(sourcePoint.transform.position, radius);
                break;
            case HitType.DamageDash:
                throw new NotImplementedException("Dash should use dash");

        }
        return hits;
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
