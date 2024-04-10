using System.Collections.Generic;
using UnityEngine;

public class CompoundCollider : MonoBehaviour
{

    public delegate void OnCompoundCollision(Collider col, bool enter);


    [HideInInspector]
    public List<Collider> colliding;

    List<FragmentCollider> fragments = new List<FragmentCollider>();

    OnCompoundCollision callback;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void addFragment(FragmentCollider f)
    {
        fragments.Add(f);
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
            if (!fragment.isColliding(col))
            {
                return;
            }
        }
        colliding.Add(col);

        callback(col, true);

    }

    public void checkCollisionExit(Collider col)
    {
        if (!colliding.Contains(col))
        {
            return;
        }
        foreach (FragmentCollider fragment in fragments)
        {
            if (!fragment.isColliding(col))
            {
                colliding.Remove(col);
                callback(col, false);
                return;
            }
        }

        

    }

    // Update is called once per frame
    void Update()
    {

    }
}
