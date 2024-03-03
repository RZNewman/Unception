using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wetstone : MonoBehaviour
{
    GameObject target;
    public GameObject bindingVis;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Interaction>().setInteraction(Interact);
    }

    void Interact(Interactor i)
    {
        GetComponent<Interaction>().stopInteraction();
        transform.GetChild(0).GetComponent<Collider>().enabled = false;
        target = i.gameObject;
        bindingVis.SetActive(false);
    }

    readonly float _targetDist = 2.5f;
    float targetDist
    {
        get
        {
            return _targetDist * transform.lossyScale.x;
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (target)
        {
            Vector3 diff = target.transform.position - transform.position;
            if(diff.magnitude > targetDist)
            {
                transform.position += diff * 1.0f * Time.fixedDeltaTime;
            }
            else
            {
                diff.x = 0;
                diff.z = 0;
                transform.position += diff * 1.0f * Time.fixedDeltaTime;
            }
        }
        
    }
}
