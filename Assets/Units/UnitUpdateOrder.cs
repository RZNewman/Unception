using Mirror;

public class UnitUpdateOrder : NetworkBehaviour
{
    Health health;
    Posture posture;
    Mezmerize mezmerize;
    Knockdown knockdown;
    Stamina stamina;
    AnimationController anim;
    PackHeal packHeal;
    EventManager eventManager;
    Gravity gravity;
    // Start is called before the first frame update
    DeterministicUpdate globalUpdate;

    [SyncVar(hook =nameof(hookRegistration))]
    bool registered;


    void Start()
    {

        globalUpdate = FindObjectOfType<DeterministicUpdate>(true);
        health = GetComponent<Health>();
        posture = GetComponent<Posture>();
        stamina = GetComponent<Stamina>();
        mezmerize = GetComponent<Mezmerize>();
        knockdown = GetComponent<Knockdown>();
        anim = GetComponent<AnimationController>();
        packHeal = GetComponent<PackHeal>();
        eventManager = GetComponent<EventManager>();
        gravity = GetComponent<Gravity>();

        eventManager.suscribeDeath((d) => setRegistration(false));

        setUpdateScripts(registered);

    }

    public void setRegistration(bool register)
    {
        if (register == registered) return;

        registered = register;
        setRegistrationHelper(register);
    }

    void hookRegistration(bool old, bool register)
    {
        if (isClientOnly)
        {
            setRegistrationHelper(register);
        }
    }

    void setRegistrationHelper(bool register)
    {
        globalUpdate = FindObjectOfType<DeterministicUpdate>(true);
        if (globalUpdate)
        {
            if (register)
            {
                globalUpdate.register(this);
            }
            else
            {
                globalUpdate.unregister(this);
            }
            setUpdateScripts(register);
        }
        
    }

    private void setUpdateScripts(bool active)
    {
        GetComponent<ControlManager>().enabled = active;
        GetComponentInChildren<UnitRotation>().enabled = active;
        GetComponentInChildren<UnitEye>().enabled = active;
    }


    public void packHealTick()
    {
        packHeal.OrderedUpdate();
    }
    public void healthTick()
    {
        health.OrderedUpdate();
    }
    public void postureTick()
    {
        posture.OrderedUpdate();
    }

    public void mezmerizeTick()
    {
        mezmerize.OrderedUpdate();
    }

    public void knockdownTick()
    {
        knockdown.OrderedUpdate();
    }
    public void staminaTick()
    {
        stamina.OrderedUpdate();
    }
    public void machineTick()
    {
        eventManager.fireTick();
    }
    public void machineTransition()
    {
        eventManager.fireTransition();
    }
    public void GravityTick()
    {
        gravity.OrderedUpdate();
    }
    public void IndicatorTick()
    {
        eventManager.fireIndicator();
    }
    public void AnimationTick()
    {
        anim.OrderedUpdate();
    }
}
