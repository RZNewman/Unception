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
	public FreeState(FreeState root) : base(root.mover)
	{

	}
	public override void enter()
	{
		
	}

	public override void exit(bool expired)
	{
		
	}

	public override void tick()
	{
		base.tick();
		UnitInput inp = mover.control.getUnitInuput();

		mover.planarVelocity = input2vec(inp.move) * mover.baseSpeed;

		
	}
	public override StateTransition transition()
	{
		
		UnitInput inp = mover.control.getUnitInuput();
		if (inp.jump)
		{
			return new StateTransition(new JumpsquatState(mover, 1f), true);
		}
		return base.transition();
	}


}
