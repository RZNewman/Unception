using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpsquatState : PlayerMovementState
{
	public JumpsquatState(UnitMovement m, float t) : base(m, t)
	{

	}
	public override void enter()
	{

	}

	public override void exit(bool expired)
	{
		if (expired)
		{
			mover.jump();
		}
		
	}

	public override void tick()
	{
		base.tick();

		
	}


}
