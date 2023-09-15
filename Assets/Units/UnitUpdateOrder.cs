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
    // Start is called before the first frame update
    void Start()
    {

        FindObjectOfType<DeterministicUpdate>().register(this);
        health = GetComponent<Health>();
        posture = GetComponent<Posture>();
        stamina = GetComponent<Stamina>();
        mezmerize = GetComponent<Mezmerize>();
        knockdown = GetComponent<Knockdown>();
        anim = GetComponent<AnimationController>();
        packHeal = GetComponent<PackHeal>();
        eventManager = GetComponent<EventManager>();


    }

    private void OnDestroy()
    {
        DeterministicUpdate master = FindObjectOfType<DeterministicUpdate>();
        if (master)
        {
            master.unregister(this);
        }

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
    public void IndicatorTick()
    {
        eventManager.fireIndicator();
    }
    public void AnimationTick()
    {
        anim.OrderedUpdate();
    }
}
