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

		float desiredAngle = -Vector2.SignedAngle(Vector2.up, inp.look);
		mover.rotate(desiredAngle, 1.0f);

		Vector3 moveDir = input2vec(inp.move);
		float inputAngle = -Vector2.SignedAngle(Vector2.up, moveDir);
		float angleDiff = Mathf.Abs(normalizeAngle(inputAngle - mover.currentLookAngle));



		float force =  1.0f;
		force *= Mathf.Lerp(1.0f, mover.backwardsMoveMultiplier, angleDiff / 180);
		if (!mover.grounded)
		{
			force *= 0.6f;
		}
		mover.move(moveDir, force,force);
		

		
	}
	public override StateTransition transition()
	{
		
		UnitInput inp = mover.control.getUnitInuput();
		if (inp.jump && mover.grounded)
		{
			return new StateTransition(new JumpsquatState(mover, mover.jumpsquatTime), true);
		}
		return base.transition();
	}


}
