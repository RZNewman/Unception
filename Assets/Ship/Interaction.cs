using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction : MonoBehaviour
{
    public string prompt;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public delegate void Interact(Interactor interactor);
    Interact action;

    public void setInteraction(Interact a)
    {
        action = a;
    }

    [Server]
    public void interact(Interactor interactor)
    {
        action(interactor);
    }


    private void OnTriggerEnter(Collider other)
    {
        other.GetComponentInParent<Interactor>().setInteraction(this, true);
    }
    private void OnTriggerExit(Collider other)
    {
        other.GetComponentInParent<Interactor>().setInteraction(this, false);
    }
}
