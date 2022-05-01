using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindState : AttackState
{
	GameObject indicator;
	AttackData attackData;
	bool hasIndicator = false;
	public WindState(Ability c, float t) : base(c, t)
	{
	}
	public WindState(Ability c, float t, AttackData data) : base(c, t)
	{
		attackData = data;
		hasIndicator = true;
	}
	public override void enter()
	{
		if (hasIndicator)
        {
			GameObject target = controller.getSpawnBody();
			indicator = Object.Instantiate(
				Resources.Load("Indicator/LineIndicator") as GameObject, 
				target.transform
				);
			IndicatorInstance i = indicator.GetComponent<IndicatorInstance>();
			i.setTrackingBody(target);
			i.reposition(attackData);
			i.setTime(currentDurration);

		}
		
	}

	public override void exit(bool expired)
	{
        if (hasIndicator)
        {
			//TODO local client destroy
            Object.Destroy(indicator);
        }
    }

}
