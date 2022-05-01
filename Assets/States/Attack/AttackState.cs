using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackState : State
{
	protected Ability controller;
	public float moveMultiplier =0.5f;
	public float lookMultiplier = 0.5f;
	public AttackState(Ability c) : base()
	{
		controller = c;
	}
	public AttackState(Ability c, float t) : base(t)
	{
		controller = c;
	}
}
