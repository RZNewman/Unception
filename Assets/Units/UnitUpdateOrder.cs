using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitUpdateOrder : MonoBehaviour
{
    UnitMovement move;
    Health health;
    Posture posture;
    // Start is called before the first frame update
    void Start()
    {
        FindObjectOfType<DeterministicUpdate>().register(this);
        health = GetComponent<Health>();
        posture = GetComponent<Posture>();
        move = GetComponent<UnitMovement>();
    }

    private void OnDestroy()
    {
        DeterministicUpdate master = FindObjectOfType<DeterministicUpdate>();
        if (master)
        {
            master.unregister(this);
        }
        
    }


    public void healthTick()
    {
        health.ServerUpdate();
    }
    public void postureTick()
    {
        posture.ServerUpdate();
    }
    public void moveTick()
    {
        move.ServerUpdate();
    }
    public void moveTransition()
    {
        move.ServerTransition();
    }
}
