using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;

public class AttackingState : PlayerMovementState
{
	AttackController castingAbility;

	public AttackingState(UnitMovement m, AttackController atk) : base(m)
	{
		castingAbility = atk;
	}

	public override void enter()
	{
		castingAbility.GetComponent<Ability>().cast();
	}

	public override void exit(bool expired)
	{
		//TODO ability kill current instance
	}
	public override void tick()
	{
		base.tick();
		UnitInput inp = mover.input;


		defaultLook(inp, castingAbility.getLookMultiplier());
		defaultMove(inp, castingAbility.getMoveMultiplier());

		castingAbility.tick();



	}
	public override StateTransition transition()
	{
		if (castingAbility.hasEnded())
		{
			return new StateTransition(new FreeState(mover), true);
		}
		return base.transition();
	}


}
