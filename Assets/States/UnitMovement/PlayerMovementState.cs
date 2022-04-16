using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;
using static Utils;

public abstract class PlayerMovementState : State
{
	protected UnitMovement mover;
	public PlayerMovementState(UnitMovement m) : base()
	{
		mover = m;
	}
	public PlayerMovementState(UnitMovement m, float t) : base(t)
	{
		mover = m;
	}

	protected void defaultLook(UnitInput inp, float mult = 1.0f)
	{
		float desiredAngle = -Vector2.SignedAngle(Vector2.up, inp.look);
		mover.rotate(desiredAngle, 1.0f * mult);
	}

	protected void defaultMove(UnitInput inp, float mult = 1.0f)
	{
		float inputAngle = -Vector2.SignedAngle(Vector2.up, inp.move);
		float angleDiff = Mathf.Abs(normalizeAngle(inputAngle - mover.currentLookAngle));

		float force = 1.0f * mult;
		if (angleDiff > 90)
		{
			force *= Mathf.Lerp(mover.sidewaysMoveMultiplier, mover.backwardsMoveMultiplier, (angleDiff - 90) / 90);
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
		mover.move(moveDir, force, force);
	}
}
