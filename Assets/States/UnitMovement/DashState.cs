using UnityEngine;
using static AttackUtils;
using static GenerateBuff;
using static GenerateDash;
using static GenerateDefense;
using static GenerateHit;
using static SpellSource;
using static UnitControl;
using static UnitMovement;
using static UnityEngine.Rendering.HableCurve;
using static Utils;

public class DashState : AttackStageState
{
    DashInstanceData opts;
    bool isAttack;
    UnitInput inpSnapshot;

    HitInstanceData hitData;
    BuffInstanceData buffData;
    DefenseInstanceData defData;
    AttackSegment segment;

    GameObject persistingAttack;

    HitList hitList = new HitList();

    public DashState(UnitMovement m, DashInstanceData o, bool attack) : base(m, o.distance / o.speed)
    {
        opts = o;
        isAttack = attack;
        inpSnapshot = UnitInput.zero();
    }
    static readonly float dashHitSpeed = 0.8f;
    public DashState(UnitMovement m, AttackSegment seg, HitInstanceData h, BuffInstanceData dataB, DefenseInstanceData def, bool attack) : base(m, dashHitSpeed)
    {
        opts = new DashInstanceData
        {
            distanceFlat = h.range,
            speedFlat = h.range / dashHitSpeed,
            control = DashControl.Forward,
            endMomentum = DashEndMomentum.Walk,
            stream = h.stream,
        };
        hitData = h;
        buffData = dataB;
        defData = def;
        segment = seg;
        isAttack = attack;
        inpSnapshot = UnitInput.zero();
    }

    public bool isHit
    {
        get
        {
            return hitData != null;
        }
    }

    public override float selfPercent
    {
        get
        {
            return 1 - currentDurration/maxDuration;
        }
    }

    public override void enter()
    {
        inpSnapshot.merge(mover.input);
        if (inpSnapshot.move == Vector2.zero)
        {
            inpSnapshot.move = vec2input(inpSnapshot.lookOffset.normalized);
        }
        mover.sound.playSound(UnitSound.UnitSoundClip.Dash);

        if (hitData != null)
        {
            foreach (SpellSource source in segment.sources)
            {
                persistingAttack = SpawnPersistent(source, mover, hitData, buffData, hitList, mover.sound.dists);
            }
            

            if (defData != null)
            {
                SpawnBuff(mover.transform, BuffMode.Shield, defData.scales, defData.duration, defData.shield(mover.GetComponent<Power>().power), defData.regen(mover.GetComponent<Power>().power));
            }
        }
    }
    public override void exit(bool expired)
    {
        if (persistingAttack)
        {
            GameObject.Destroy(persistingAttack);
        }
        if (expired)
        {
            switch (opts.endMomentum)
            {
                case DashEndMomentum.Full:
                    break;
                case DashEndMomentum.Walk:
                    mover.setToWalkSpeed();
                    break;
                case DashEndMomentum.Stop:
                    mover.stop();
                    break;
            }
        }

    }

    public override StateTransition transition()
    {
        if (isAttack)
        {
            return base.transition();
        }
        if (mover.isIncapacitated)
        {
            return new StateTransition(new StunnedState(mover), true);
        }
        return base.transition();
    }
    public DashInstanceData getSource()
    {
        return opts;
    }

    public HitInstanceData getHit()
    {
        return hitData;
    }

    public ShapeData getShapeData()
    {
        return AttackUtils.getShapeData(hitData.shape, segment.capsuleSize, hitData.range, hitData.length, hitData.width, usesRangeForHitbox(hitData.type));
    }

    public override void tick()
    {
        base.tick();

        mover.dash(inpSnapshot, opts);

    }
    public override IndicatorOffsets GetIndicatorOffsets()
    {
        return new IndicatorOffsets
        {
            distance = Vector3.forward * opts.distance * (opts.control == DashControl.Backward ? -1 : 1) * currentDurration / maxDuration,
            time = currentDurration,
        };
    }

    protected override float tickSpeedMult()
    {
        return opts.castSpeedMultiplier;
    }
}
