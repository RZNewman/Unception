using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;

public class AttackBlock : ScriptableObject
{
	public AttackGenerationData source;
	public AttackInstanceData instance;
	public bool scales;

	public List<PlayerMovementState> buildStates(UnitMovement controller)
	{
		List<PlayerMovementState> states = new List<PlayerMovementState>();

		InstanceDataPreview preview = null;
		for(int i = instance.stages.Length-1; i >= 0; i--)
        {
			InstanceData data = instance.stages[i];
			switch (data)
            {
				case WindInstanceData w:
					states.Add(new WindState(controller, w, preview));
					preview = null;
					break;
				case InstanceDataPreview pre:
					preview = pre;
					switch (pre)
                    {
						case HitInstanceData hit:
							states.Add(new ActionState(controller, hit));
							break;
                    }
					break;

			}
        }

		states.Reverse();
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
