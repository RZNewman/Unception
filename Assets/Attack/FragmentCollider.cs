using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FragmentCollider : MonoBehaviour
{
    CompoundCollider comp;

    public bool subtract = false;

    [HideInInspector]
    List<Collider> colliding = new List<Collider>();

    Dictionary<Collider,bool> fullyInside = new Dictionary<Collider, bool>();
    public bool isColliding(Collider col)
    {

        return subtract ? !fullyInside.ContainsKey(col) || !fullyInside[col] : colliding.Contains(col);

    }
    // Start is called before the first frame update
    void Awake()
    {
        comp = GetComponentInParent<CompoundCollider>();
        comp.addFragment(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        colliding.Add(other);
        if (subtract)
        {
            fullyInside.Add(other, isFullyInside(other));
        }
        comp.checkCollisionEnter(other);
    }
    private void OnTriggerExit(Collider other)
    {
        colliding.Remove(other);
        if (subtract)
        {
            fullyInside.Remove(other);
        }
        comp.checkCollisionExit(other);
    }

    private void FixedUpdate()
    {
        foreach(Collider col in fullyInside.Keys.ToList())
        {
            fullyInside[col] = isFullyInside(col);
        }
    }
    bool isFullyInside(Collider col)
    {
        SphereCollider myCol = GetComponent<SphereCollider>();

        return col switch
        {
            SphereCollider s => (s.gameObject.transform.position - transform.position).magnitude + s.radius < myCol.radius,
            CapsuleCollider c => false,
            _ => throw new NotImplementedException(),
        };
    }
}
