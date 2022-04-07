using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackState : State
{
	public AttackState() : base()
	{
	}
	public AttackState(float t) : base(t)
	{
	}
}
