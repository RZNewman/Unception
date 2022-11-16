using UnityEngine;
using static UnitControl;

public class StunnedState : PlayerMovementState, BarValue
{
    public StunnedState(UnitMovement m) : base(m)
    {

    }

    public StunnedState(UnitMovement m, float t) : base(m, t)
    {
        //used for sturn duration overrides
    }

    public override void tick()
    {
        base.tick();

        //no look change
        //defaultLook(inp);
        mover.move(UnitInput.zero(), 0.2f);

    }

    public override StateTransition transition()
    {
        if (currentDurration <= 0 && !mover.posture.isStunned)
        {
            return new StateTransition(new FreeState(mover), true);
        }
        return new StateTransition(null, false);
    }
    public override void enter()
    {
        mover.sound.playSound(mover.local.isLocalUnit ? UnitSound.UnitSoundClip.LocalStun : UnitSound.UnitSoundClip.Stun);
    }

    public override void exit(bool expired)
    {
        mover.GetComponent<Cast>().removeTarget(this);
    }

    public BarValue.BarData getBarFill()
    {
        return new BarValue.BarData
        {
            color = GameColors.Stunned,
            fillPercent = Mathf.Clamp01(currentDurration / maxDuration),
            active = true,
        };
    }
}
