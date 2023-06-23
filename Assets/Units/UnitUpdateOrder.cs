using Mirror;

public class UnitUpdateOrder : NetworkBehaviour
{
    UnitMovement move;
    Health health;
    Posture posture;
    Stamina stamina;
    Cast cast;
    AnimationController anim;
    PackHeal packHeal;
    // Start is called before the first frame update
    void Start()
    {

        FindObjectOfType<DeterministicUpdate>().register(this);
        health = GetComponent<Health>();
        posture = GetComponent<Posture>();
        move = GetComponent<UnitMovement>();
        stamina = GetComponent<Stamina>();
        cast = GetComponent<Cast>();
        anim = GetComponent<AnimationController>();
        packHeal = GetComponent<PackHeal>();


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
    public void staminaTick()
    {
        stamina.OrderedUpdate();
    }
    public void moveTick()
    {
        move.OrderedUpdate();
        cast.OrderedUpdate();
    }
    public void moveTransition()
    {
        move.OrderedTransition();
        cast.OrderedTransition();
    }
    public void IndicatorTick()
    {
        cast.OrderedIndicator();
    }
    public void AnimationTick()
    {
        anim.OrderedUpdate();
    }
}
