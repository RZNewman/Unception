using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction : MonoBehaviour
{
    public string prompt;

    bool canInteract = true;
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
    public delegate bool Condidtion(Interactor i);
    Condidtion condidtion = (_) => true;

    public void setInteraction(Interact a)
    {
        action = a;
    }
    public void setCondition(Condidtion c)
    {
        condidtion = c;
    }

    [Server]
    public void interact(Interactor interactor)
    {
        action(interactor);
    }

    public bool conditionMet(Interactor interactor)
    {
        return condidtion(interactor);
    }

    List<Interactor> interactors = new List<Interactor>();

    private void OnTriggerEnter(Collider other)
    {
        if (canInteract)
        {
            Interactor i = other.GetComponentInParent<Interactor>();
            if (i)
            {
                setOverlap(i, true);
            }   
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (canInteract)
        {
            Interactor i = other.GetComponentInParent<Interactor>();
            if (i)
            {
                setOverlap(i, false);
            }
        }
    }

    public void setInteractable(bool i)
    {
        clear();
        canInteract = i;
    }

    void clear()
    {
        foreach(Interactor i  in interactors.ToArray())
        {
            setOverlap(i, false);
        }
    }

    void setOverlap(Interactor i, bool isInteracting)
    {
        i.setInteraction(this, isInteracting);
        if (isInteracting)
        {
            interactors.Add(i);
        }
        else
        {
            interactors.Remove(i);
        }
        
    }
}
