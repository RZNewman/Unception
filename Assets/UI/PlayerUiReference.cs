using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUiReference : MonoBehaviour
{
    PlayerGhost target;
    public GameObject extraLife;
    public void setTarget(PlayerGhost t)
    {
        target = t;

    }
    

    // Update is called once per frame
    void Update()
    {
        if (target)
        {
            if (extraLife.activeSelf != target.extraLife)
            {
                extraLife.SetActive(target.extraLife);
            }
            
        }
    }
}
