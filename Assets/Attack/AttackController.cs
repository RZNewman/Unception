using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    AttackBlock attackFormat;
    StateMachine<AttackState> attackMachine;
    bool ended = false;

    List<AttackState> currentStates;
    GameObject rotatingBody;

	private void Start()
	{
        rotatingBody = transform.parent.GetComponentInChildren<UnitRotation>().gameObject;
	}

    public GameObject getSpawnBody()
	{
        return rotatingBody;
	}
	// Start is called before the first frame update
	public void buildAttack()
    {
        currentStates = attackFormat.buildStates(this);
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
        ended = true;
        return new WindState(this,1f);
	}

    public void setFormat(AttackBlock b)
	{
        attackFormat = b;
	}

    public float getLookMultiplier()
	{
        return attackMachine.state().lookMultiplier;
	}

    public float getMoveMultiplier()
    {
        return attackMachine.state().moveMultiplier;
    }


    public void tick()
	{
        attackMachine.tick();
	}

    public bool hasEnded()
	{
        return ended;
	}
}
