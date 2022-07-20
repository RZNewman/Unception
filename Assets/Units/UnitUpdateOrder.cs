using Mirror;

public class UnitUpdateOrder : NetworkBehaviour
{
    UnitMovement move;
    Health health;
    Posture posture;
    Stamina stamina;
    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
        {
            FindObjectOfType<DeterministicUpdate>().register(this);
            health = GetComponent<Health>();
            posture = GetComponent<Posture>();
            move = GetComponent<UnitMovement>();
            stamina = GetComponent<Stamina>();
        }

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
    public void staminaTick()
    {
        stamina.ServerUpdate();
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
