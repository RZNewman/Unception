using UnityEngine;
using static GenerateDash;
using static UnitControl;
using static UnitMovement;

public class DashState : AttackStageState
{
    DashInstanceData opts;
    bool isAttack;
    UnitInput inpSnapshot;


    public DashState(UnitMovement m, DashInstanceData o, bool attack = false) : base(m, o.distance / o.speed)
    {
        opts = o;
        isAttack = attack;
    }

    public override void enter()
    {
        inpSnapshot = mover.input;
        mover.sound.playSound(UnitSound.UnitSoundClip.Dash);
    }
    public override void exit(bool expired)
    {
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
        if (mover.posture.isStunned)
        {
            return new StateTransition(new StunnedState(mover), true);
        }
        return base.transition();
    }
    public DashInstanceData getSource()
    {
        return opts;
    }

    public override void tick()
    {
        base.tick();

        mover.dash(inpSnapshot, opts.speed, opts.control);

    }
    public override Cast.IndicatorOffsets GetIndicatorOffsets()
    {
        return new Cast.IndicatorOffsets
        {
            distance = Vector3.forward * opts.distance * (opts.control == DashControl.Backward ? -1 : 1) * currentDurration / (opts.distance / opts.speed),
            time = currentDurration,
        };
    }
}
