using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
    AttackController controller;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<AttackController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void cast()
	{
        controller.buildAttack();
	}

}
