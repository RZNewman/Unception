using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;

public class AttackingState : PlayerMovementState
{
	Ability castingAbility;

	StateMachine<AttackState> attackMachine;
	bool ended = false;

	List<AttackState> currentStates;
	

	public AttackingState(UnitMovement m, Ability atk) : base(m)
	{
		castingAbility = atk;
	}

	public override void enter()
	{
		currentStates = castingAbility.cast();
		attackMachine = new StateMachine<AttackState>(getNextState);
		
	}

	public override void exit(bool expired)
	{
		//TODO ability kill current instance
	}
	public override void tick()
	{
		base.tick();
		UnitInput inp = mover.input;


		mover.rotate(inp, attackMachine.state().lookMultiplier);
		mover.move(inp, attackMachine.state().moveMultiplier, attackMachine.state().moveMultiplier);

		
		attackMachine.tick();



	}
	public override StateTransition transition()
	{
		if (mover.posture.isStunned)
		{
			return new StateTransition(new StunnedState(mover), true);
		}
		attackMachine.transition();
		if (ended)
		{
			return new StateTransition(new FreeState(mover), true);
		}
		return base.transition();
	}

	AttackState getNextState()
	{
		if (currentStates.Count > 0)
		{
			AttackState s = currentStates[0];
			currentStates.RemoveAt(0);
			return s;
		}
		ended = true;
		return new WindState(castingAbility, 1f);
	}



}
