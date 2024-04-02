using System.Collections.Generic;
using UnityEngine;

public class FragmentCollider : MonoBehaviour
{
    CompoundCollider comp;

    public bool subtract = false;

    [HideInInspector]
    public List<Collider> colliding = new List<Collider>();
    // Start is called before the first frame update
    void Awake()
    {
        comp = GetComponentInParent<CompoundCollider>();
        comp.addFragment(this);
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
