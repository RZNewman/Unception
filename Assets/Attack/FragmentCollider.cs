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
            comp.checkCollisionExit(other);
        }
        else
        {
            comp.checkCollisionEnter(other);
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        colliding.Remove(other);
        if (subtract)
        {
            fullyInside.Remove(other);
            comp.checkCollisionEnter(other);
        }
        else
        {
            comp.checkCollisionExit(other);
        }
        
    }

    private void FixedUpdate()
    {
        foreach(Collider col in fullyInside.Keys.ToList())
        {
            if (col)
            {
                fullyInside[col] = isFullyInside(col);
            }
            else
            {
                fullyInside.Remove(col);
            }
            
        }
    }
    bool isFullyInside(Collider col)
    {
        SphereCollider myCol = GetComponent<SphereCollider>();

        return col switch
        {
            SphereCollider s => myCol.FullyInside(s),
            CapsuleCollider c => myCol.FullyInside(c),
            BoxCollider b => myCol.FullyInside(b),
            _ => throw new NotImplementedException(),
        };
    }
}
