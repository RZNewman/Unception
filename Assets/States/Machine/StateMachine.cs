using System;
using static State;

public class StateMachine<T> where T : State
{
    T currentState;
    Func<T> defaultBuilder;
    public StateMachine(Func<T> defaultB)
    {
        defaultBuilder = defaultB;
        currentState = defaultBuilder();
        currentState.enter();
    }
    public void transition()
    {
        StateTransition t = currentState.transition();
        if (t.shouldTrasition)
        {
            T nextState;
            if (t.nextState == null)
            {
                nextState = defaultBuilder();
            }
            else
            {
                nextState = (T)t.nextState;
            }
            currentState.exit(t.expired);
            currentState = nextState;
            currentState.enter();
        }
    }

    public void tick()
    {

        currentState.tick();

    }
    public void exit()
    {
        currentState.exit(false);
    }

    public T state()
    {
        return currentState;
    }
}
