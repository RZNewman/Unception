using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;

public class AttackBlock : ScriptableObject
{
	public float windup;
	public float winddown;
	public float cooldown;
	public AttackData data;
	public AttackGenerationValues source;
	public bool scales;

	public List<PlayerMovementState> buildStates(UnitMovement controller)
	{
		List<PlayerMovementState> states = new List<PlayerMovementState>();

		states.Add(new WindState(controller, windup, data));
		states.Add(new ActionState(controller, data));
		states.Add(new WindState(controller,winddown));
		return states;
	}

    public float getCooldown()
    {
        return cooldown;
    }

    public AiHandler.EffectiveDistance GetEffectiveDistance()
    {
        return data.GetEffectiveDistance();
    }
}
