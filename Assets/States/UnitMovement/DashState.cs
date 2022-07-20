using UnityEngine;
using static UnitControl;
using static UnitMovement;

public class DashState : PlayerMovementState
{
    DashOptions opts;
    bool isAttack;
    UnitInput inpSnapshot;

    public enum DashEndMomentum
    {
        Full,
        Walk,
        Stop
    }
    public struct DashOptions
    {
        public float dashSpeed;
        public float dashDistance;
        public DashControl control;
        public DashEndMomentum ending;
    }
    public DashState(UnitMovement m, DashOptions o, bool attack = false) : base(m, o.dashDistance / o.dashSpeed)
    {
        opts = o;
        isAttack = attack;
    }

    public override void enter()
    {
        inpSnapshot = mover.input;
    }
    public override void exit(bool expired)
    {
        if (expired)
        {
            switch (opts.ending)
            {
                case DashEndMomentum.Full:
                    break;
                case DashEndMomentum.Walk:
                    mover.setToWalkSpeed();
                    break;
                case DashEndMomentum.Stop:
                    mover.planarVelocity = Vector3.zero;
                    break;
                default:
                    mover.planarVelocity = Vector3.zero;
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

    public override void tick()
    {
        base.tick();

        mover.dash(inpSnapshot, opts.dashSpeed, opts.control);

    }
}
