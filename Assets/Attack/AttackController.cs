using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    AttackBlock attackFormat;
    StateMachine<AttackState> attackMachine;

    List<AttackState> currentStates;
    // Start is called before the first frame update
    public void buildAttack()
    {
        currentStates = attackFormat.buildStates();
        attackMachine = new StateMachine<AttackState>(getNextState);
    }

    AttackState getNextState()
	{
		if (currentStates.Count > 0)
		{
            AttackState s = currentStates[0];
            currentStates.RemoveAt(0);
            return s;
		}
        //Set Termination
        return new WindState(1f);
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
