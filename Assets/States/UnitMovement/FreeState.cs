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
		UnitInput inp = mover.input;

		float desiredAngle = -Vector2.SignedAngle(Vector2.up, inp.look);
		mover.rotate(desiredAngle, 1.0f);

		
		float inputAngle = -Vector2.SignedAngle(Vector2.up, inp.move);
		float angleDiff = Mathf.Abs(normalizeAngle(inputAngle - mover.currentLookAngle));

		float force =  1.0f;
		if (angleDiff > 90)
		{
			force *= Mathf.Lerp(mover.sidewaysMoveMultiplier, mover.backwardsMoveMultiplier, (angleDiff-90) / 90);
		}
		else
		{
			force *= Mathf.Lerp(1.0f, mover.sidewaysMoveMultiplier, angleDiff / 90);
		}
		
		if (!mover.grounded)
		{
			force *= 0.6f;
		}
		Vector3 moveDir = input2vec(inp.move);
		mover.move(moveDir, force,force);
		

		
	}
	public override StateTransition transition()
	{
		
		UnitInput inp = mover.input;
		if (inp.jump && mover.grounded)
		{
			return new StateTransition(new JumpsquatState(mover, mover.jumpsquatTime), true);
		}
		return base.transition();
	}


}
