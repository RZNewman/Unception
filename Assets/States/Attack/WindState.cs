using Mirror;
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
			//TODO Spawn indicator
			indicator = Object.Instantiate(
				Resources.Load("Indicator/LineIndicator") as GameObject, 
				target.transform
				);
			IndicatorInstance i = indicator.GetComponent<IndicatorInstance>();
			i.setTeam(controller.GetComponentInParent<TeamOwnership>().getTeam());
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
        if (hasIndicator)
        {
			//TODO local client destroy
            Object.Destroy(indicator);
        }
    }

}
