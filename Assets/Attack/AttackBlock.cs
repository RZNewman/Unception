using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;

public class AttackBlock : ScriptableObject
{
	public GenerationAttack source;
	public AttackInstanceData instance;
	public bool scales;

	public List<PlayerMovementState> buildStates(UnitMovement controller)
	{
		List<PlayerMovementState> states = new List<PlayerMovementState>();

		states.Add(new WindState(controller, instance.windup, instance.hit));
		states.Add(new ActionState(controller, instance.hit));
		states.Add(new WindState(controller, instance.winddown));
		return states;
	}

    public float getCooldown()
    {
        return instance.cooldown;
    }

    public AiHandler.EffectiveDistance GetEffectiveDistance()
    {
        return instance.hit.GetEffectiveDistance();
    }
}
