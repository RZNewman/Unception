using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggroHandler : MonoBehaviour
{
	public float aggroRadius = 15f;

	[HideInInspector]
	public List<GameObject> aggro;

	Pack pack;

	private void Start()
	{
		aggro = new List<GameObject>();
		SphereCollider col = GetComponent<SphereCollider>();
		col.radius = aggroRadius;
		pack = transform.parent.GetComponentInChildren<PackTag>().owner;
        if (pack)
        {
			pack.addToPack(this);
		}
		
	}
	private void OnTriggerEnter(Collider other)
	{
		GameObject them = other.gameObject;
		uint thierTeam = them.GetComponentInParent<TeamOwnership>().getTeam();
		uint myTeam = GetComponentInParent<TeamOwnership>().getTeam();
		if (myTeam != thierTeam)
		{
			if (!aggro.Contains(them))
			{
				aggro.Add(them);
                if (pack)
                {
					pack.packAggro(them);
                }
			}

		}
	}
	public void addAggro(GameObject target)
    {
		if (!aggro.Contains(target))
		{
			aggro.Add(target);

		}
	}

	public GameObject getTopTarget()
	{
		if (aggro.Count > 0)
		{
			return aggro[0];
		}
		return null;
	}
}
