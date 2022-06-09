using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;
using static Utils;

public class AiHandler : MonoBehaviour, UnitControl
{
	UnitInput currentInput;
	AggroHandler aggro;
	UnitMovement mover;
	GameObject rotatingBody;

	public struct EffectiveDistance
    {
		public float distance;
		public float angle;
		public EffectiveDistance(float distance, float angle)
        {
			this.distance = distance;
			this.angle = angle;
        }
    }
	public UnitInput getUnitInuput()
	{
		return currentInput;
	}
	public void init()
	{
		currentInput = new UnitInput();
		currentInput.reset();
		aggro = GetComponent<AggroHandler>();
		mover = GetComponentInParent<UnitMovement>();
		rotatingBody = mover.GetComponentInChildren<UnitRotation>().gameObject;
	}
    public void reset()
    {
		currentInput.reset();
	}
    public void refreshInput()
	{
		//Get current target and move to it
		if (aggro)
		{
			GameObject target = aggro.getTopTarget();
			if (target)
			{
				Vector3 rawDiff = target.transform.position - transform.position;
				Vector3 planarDiff = rawDiff;
				planarDiff.y = 0;
				Vector3 inpDiff = planarDiff;
				inpDiff.Normalize();
				Vector2 inpVec = vec2input(inpDiff);
				currentInput.move = inpVec;
				currentInput.look = inpVec;

				float edgeDiffMag  = planarDiff.magnitude - rotatingBody.GetComponentInChildren<Size>().scaledRadius - target.GetComponent<Size>().scaledRadius;

				EffectiveDistance eff = GetComponentInParent<AbiltyList>().getAbility(0).GetEffectiveDistance();
				if (edgeDiffMag <= eff.distance && Vector3.Angle(planarDiff, rotatingBody.transform.forward) < eff.angle) 
                {
					currentInput.attacks = new AttackKey[] {AttackKey.One};
                }
                else
                {
					currentInput.attacks = new AttackKey[0];
                }

			}
            else
            {
				currentInput.reset();
            }
		}
		
	}

}
