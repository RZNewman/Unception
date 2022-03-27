using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static State;

public class StateMachine<T> where T: State
{
    T currentState;
    T defaultState;
    public StateMachine(T baseState){
        defaultState = baseState;
        currentState = (T)Activator.CreateInstance(baseState.GetType(), baseState); ;
        currentState.enter();
    }

    public void tick()
	{
        StateTransition t = currentState.transition();
		if (t.shouldTrasition)
		{
            T nextState;
            if(t.nextState == null)
			{
                nextState = (T)Activator.CreateInstance(defaultState.GetType(), defaultState);
            }
			else
			{
                nextState = (T)t.nextState;
			}
            currentState.exit(t.expired);
            currentState = nextState;
            currentState.enter();
		}
        currentState.tick();

    }
}
