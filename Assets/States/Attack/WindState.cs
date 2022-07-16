using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;

public class WindState : AttackState
{
	GameObject indicator;
	InstanceDataPreview previewData;
	bool hasIndicator = false;

	public WindState(UnitMovement m, float d) : base(m, d)
	{
		//Only for defaults
		hasIndicator = false;
	}

	public WindState(UnitMovement m, WindInstanceData d) : base(m, d.duration)
	{
		hasIndicator = false;
		moveMultiplier = d.moveMult;
		lookMultiplier = d.turnMult;
	}

	public WindState(UnitMovement m, WindInstanceData d, InstanceDataPreview data) : base(m, d.duration)
	{
		previewData = data;
		hasIndicator = true;
		moveMultiplier = d.moveMult;
		lookMultiplier = d.turnMult;
	}
	public override void enter()
	{
		GameObject target = mover.getSpawnBody();
		target.GetComponentInParent<Cast>().setTarget(this);
		if (hasIndicator)
        {
			
			buildIndicator(target);

		}
		
	}

	void buildIndicator(GameObject target)
    {
        switch (previewData)
        {
			case HitInstanceData attackData:
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
				break;
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
