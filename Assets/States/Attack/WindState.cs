using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindState : AttackState
{
	GameObject indicator;
	AttackData attackData;
	bool hasIndicator = false;
	
	public WindState(UnitMovement m, float t, AttackData data = null) : base(m, t)
	{
		attackData = data;
		hasIndicator = data != null;
	}
	public override void enter()
	{
		GameObject target = mover.getSpawnBody();
		target.GetComponentInParent<Cast>().setTarget(this);
		if (hasIndicator)
        {
			indicator = Object.Instantiate(
				Resources.Load("Indicator/LineIndicator") as GameObject, 
				target.transform
				);
			IndicatorInstance i = indicator.GetComponent<IndicatorInstance>();
			i.setTeam(mover.GetComponent<TeamOwnership>().getTeam());
			i.reposition(attackData);
			i.setTime(currentDurration);
			ClientAdoption adoptee = indicator.GetComponent<ClientAdoption>();
			adoptee.parent = target.GetComponentInParent<NetworkIdentity>().gameObject;
			adoptee.useSubBody = true;
			NetworkServer.Spawn(indicator);


		}
		
	}

	public override void exit(bool expired)
	{
		GameObject target = mover.getSpawnBody();
		target.GetComponentInParent<Cast>().removeTarget();
		if (hasIndicator)
        {
            Object.Destroy(indicator);
        }
    }

	public BarValue.BarData getProgress()
    {
		return new BarValue.BarData
		{
			color = hasIndicator ? Color.cyan: new Color(0,0.6f,1),
			fillPercent = Mathf.Clamp01(hasIndicator ? 1 - (currentDurration / maxDuration): currentDurration / maxDuration),
			active = true,
		};
	}
}
