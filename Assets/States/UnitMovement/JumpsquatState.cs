using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpsquatState : PlayerMovementState
{
	public JumpsquatState(UnitMovement m, float t) : base(m, t)
	{

	}
	public override void exit(bool expired)
	{
		if (expired)
		{
			mover.jump();
		}
		
	}
	public override StateTransition transition()
	{
		if (mover.posture.isStunned)
		{
			return new StateTransition(new StunnedState(mover), true);
		}
		return base.transition();
	}


}
