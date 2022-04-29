using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackBlock : ScriptableObject
{
	public abstract List<AttackState> buildStates(AttackController controller);
}
