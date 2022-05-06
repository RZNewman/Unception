using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;
using static Utils;

public class FreeState : PlayerMovementState
{
	public FreeState(UnitMovement m) : base(m)
	{

	}

	public override void tick()
	{
		base.tick();
		UnitInput inp = mover.input;

		mover.rotate(inp);
		mover.move(inp);

	}
	public override StateTransition transition()
	{
        if (mover.posture.isStunned)
        {
			return new StateTransition(new StunnedState(mover), true);
        }
		UnitInput inp = mover.input;
		AttackKey[] atks = inp.attacks;
		if (atks.Length > 0)
		{
			for(int i = 0; i < atks.Length; i++)
            {
				Ability a = mover.GetComponent<AbiltyList>().getAbility(inp.attacks[i]);
				if (a.ready)
				{
					return new StateTransition(new AttackingState(mover, a), true);
				}
			}
            
		}
		if (inp.jump && mover.grounded)
		{
			return new StateTransition(new JumpsquatState(mover, mover.props.jumpsquatTime), true);
		}
		return base.transition();
	}


}
