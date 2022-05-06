using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AiHandler;

public abstract class AttackBlock : ScriptableObject
{
	public abstract List<AttackState> buildStates(Ability controller);

	public abstract EffectiveDistance GetEffectiveDistance();
}
