using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBlock : ScriptableObject
{
	public float windup;
	public float winddown;
	public float cooldown;
	public AttackData data;

	public List<AttackState> buildStates(Ability controller)
	{
		List<AttackState> states = new List<AttackState>();

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
