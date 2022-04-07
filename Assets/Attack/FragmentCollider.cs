using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragmentCollider : MonoBehaviour
{
    CompoundCollider comp;

	[HideInInspector]
    public List<Collider> colliding;
    // Start is called before the first frame update
    void Start()
    {
        comp = GetComponentInParent<CompoundCollider>();
        colliding = new List<Collider>();
    }

	private void OnTriggerEnter(Collider other)
	{
		colliding.Add(other);
		comp.checkCollisionEnter(other);
	}
	private void OnTriggerExit(Collider other)
	{
		colliding.Remove(other);
		comp.checkCollisionExit(other);
	}
}
