using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggroHandler : MonoBehaviour
{
	public float aggroRadius = 15f;

	[HideInInspector]
	public List<GameObject> aggro;

	private void Start()
	{
		aggro = new List<GameObject>();
		SphereCollider col = GetComponent<SphereCollider>();
		col.radius = aggroRadius;
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
	
			}

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
