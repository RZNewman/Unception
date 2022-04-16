using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompoundCollider : MonoBehaviour
{

    public delegate void OnCompoundCollision(Collider col);


    [HideInInspector]
    public List<Collider> colliding;

    FragmentCollider[] fragments;

    OnCompoundCollision callback;
    // Start is called before the first frame update
    void Start()
    {
        fragments = GetComponentsInChildren<FragmentCollider>();
    }

    public void setCallback(OnCompoundCollision call)
	{
        callback = call;
	}

    public void checkCollisionEnter(Collider col)
	{
        if (colliding.Contains(col))
		{
            return;
		}
        foreach (FragmentCollider fragment in fragments)
        {
            if (!colliding.Contains(col))
            {
                return;
            }
        }
        colliding.Add(col);
        Debug.Log("Collide");
        callback(col);

    }

    public void checkCollisionExit(Collider col)
    {
        if (!colliding.Contains(col))
        {
            return;
        }
        foreach (FragmentCollider fragment in fragments)
        {
            if (colliding.Contains(col))
            {
                return;
            }
        }
        colliding.Remove(col);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
