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
        //Debug.Log(name + "Check");
        //foreach(Collider c in colliding)
        //{
        //    Debug.Log(c);
        //}
        return subtract ? !fullyInside.ContainsKey(col) || !fullyInside[col] : colliding.Contains(col);

    }
    // Start is called before the first frame update
    void Awake()
    {
        comp = GetComponentInParent<CompoundCollider>();
        comp.addFragment(this);
        name = "Fragment" + Random.Range(1, 1000);
    }

    private void OnTriggerEnter(Collider other)
    {
        //TODO this shouldnt be able to hit twice...
        if (!colliding.Contains(other))
        {
            colliding.Add(other);
        }
        //colliding.Add(other);
        if (subtract)
        {
            //Debug.Log("x sub" + name + " - " + other.name);
            fullyInside.Add(other, isFullyInside(other));
            comp.checkCollisionExit(other);
        }
        else
        {
            //Debug.Log("e" + name + " - " + other.name);
            comp.checkCollisionEnter(other);
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        colliding.Remove(other);
        if (subtract)
        {
            //Debug.Log("e - sub" + name + " - " + other.name);
            fullyInside.Remove(other);
            comp.checkCollisionEnter(other);
        }
        else
        {
            //Debug.Log("x"+ name +" - "+ other.name);
            comp.checkCollisionExit(other);
        }
        
    }

    private void FixedUpdate()
    {
        foreach(Collider col in fullyInside.Keys.ToList())
        {
            if (col)
            {
                bool last = fullyInside[col];
                fullyInside[col] = isFullyInside(col);
                if(last != fullyInside[col])
                {
                    if (last)
                    {
                        comp.checkCollisionEnter(col);
                    }
                    else
                    {
                        comp.checkCollisionExit(col);
                    }
                }
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
            _ => throw new System.NotImplementedException(),
        };
    }
}
