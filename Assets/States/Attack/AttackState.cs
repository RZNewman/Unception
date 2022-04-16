using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackState : State
{
	protected AttackController controller;
	public float moveMultiplier =0.5f;
	public float lookMultiplier = 0.5f;
	public AttackState(AttackController c) : base()
	{
		controller = c;
	}
	public AttackState(AttackController c, float t) : base(t)
	{
		controller = c;
	}
}
