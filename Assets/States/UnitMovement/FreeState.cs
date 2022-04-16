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

		defaultLook(inp);
		defaultMove(inp);






	}
	public override StateTransition transition()
	{
		
		UnitInput inp = mover.input;
		AttackKey[] atks = inp.attacks;
		if (atks.Length > 0)
		{
			//TODO eat keys and find ability off cooldown
			AttackController a = mover.GetComponent<AbiltyList>().getAbility(inp.attacks[0]).GetComponent<AttackController>();
			return new StateTransition(new AttackingState(mover, a), true);
		}
		if (inp.jump && mover.grounded)
		{
			return new StateTransition(new JumpsquatState(mover, mover.jumpsquatTime), true);
		}
		return base.transition();
	}


}
