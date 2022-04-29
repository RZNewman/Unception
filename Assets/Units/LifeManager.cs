using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeManager : MonoBehaviour
{
    GameObject unitBody;

    private void Start()
    {
        unitBody = GetComponentInChildren<Size>().gameObject;
    }
    bool isPlayer
    {
        get
        {
            return GetComponent<ControlManager>().isPlayer;
        }
    }
    bool isDead = false;

    public bool IsDead
    {
        get { return isDead; }
    }
    
    public void die()
    {
        isDead = true;
        if (!isPlayer)
        {
            Destroy(gameObject);
        }
        else
        {
            Destroy(unitBody);
        }

    }
}
