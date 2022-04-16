using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;
using static Utils;

public class AiHandler : MonoBehaviour, UnitControl
{
	UnitInput currentInput;
	AggroHandler aggro;
	public UnitInput getUnitInuput()
	{
		return currentInput;
	}
	void Start()
	{
		currentInput = new UnitInput();
		currentInput.reset();
		aggro = GetComponent<AggroHandler>();
	}
	public void refreshInput()
	{
		//Get current target and move to it
		if (aggro)
		{
			GameObject target = aggro.getTopTarget();
			if (target)
			{
				Vector3 diff = target.transform.position - transform.position;
				diff.y = 0;
				diff.Normalize();
				Vector2 inpVec = vec2input(diff);
				currentInput.move = inpVec;
				currentInput.look = inpVec;
			}
		}
		
	}

}
