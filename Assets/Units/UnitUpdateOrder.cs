using Mirror;
using System.Collections;
using UnityEngine;

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
    UnitMovement mover;
    LifeManager life;
    // Start is called before the first frame update
    DeterministicUpdate globalUpdate;

    enum ActiveState
    {
        None,
        Active,
        AwaitStop
    }

    [SyncVar(hook =nameof(hookRegistration))]
    ActiveState state = ActiveState.None;


    void Start()
    {

        globalUpdate = DeterministicUpdate.Determ;
        health = GetComponent<Health>();
        posture = GetComponent<Posture>();
        stamina = GetComponent<Stamina>();
        mezmerize = GetComponent<Mezmerize>();
        knockdown = GetComponent<Knockdown>();
        anim = GetComponent<AnimationController>();
        packHeal = GetComponent<PackHeal>();
        eventManager = GetComponent<EventManager>();
        gravity = GetComponent<Gravity>();
        gravity.turnOffGrav();
        mover = GetComponent<UnitMovement>();
        life = GetComponent<LifeManager>();
        eventManager.suscribeDeath((d) => {
            setRegistration(false, true);
            });
        setUpdateScripts();
    }

    public void logUnit()
    {
        Debug.Log(name);
    }

    IEnumerator delayUnregister(DeterministicUpdate globalUpdate)
    {
        while (!mover.grounded)
        {
            yield return null;
        }
        if (state != ActiveState.AwaitStop) yield break;

        state = ActiveState.None;
        setRegistrationHelper();

    }


    public void setRegistration(bool register, bool death = false)
    {
        ActiveState target = (register, death) switch
        {
            (true, _) => ActiveState.Active,
            (_, true) => ActiveState.None,
            _ => ActiveState.AwaitStop,
        };
        ActiveState old = state;

        if (target == old) return;
        if (target == ActiveState.AwaitStop && old == ActiveState.None) return;

        state = target;
        if(target == ActiveState.Active && old == ActiveState.AwaitStop) return;
        setRegistrationHelper();
    }

    void hookRegistration(ActiveState old, ActiveState state)
    {
        if (isClientOnly)
        {
            if(state == ActiveState.AwaitStop)
            {
                return;
            }

            setRegistrationHelper();
        }
    }

    void setRegistrationHelper()
    {
        globalUpdate = FindObjectOfType<DeterministicUpdate>(true);
        if (globalUpdate)
        {
            switch (state)
            {
                case ActiveState.Active:
                    globalUpdate.register(this);
                    setUpdateScripts();
                    break;
                case ActiveState.None:
                    globalUpdate.unregister(this);
                    setUpdateScripts();
                    break;
                case ActiveState.AwaitStop:
                    StartCoroutine(delayUnregister(globalUpdate));
                    break;
            }
            
        }
        
    }

    private void setUpdateScripts()
    {
        bool active = state != ActiveState.None;

        GetComponent<ControlManager>().enabled = active;
        GetComponentInChildren<UnitRotation>().enabled = active;
        GetComponentInChildren<UnitEye>().enabled = active;
        GetComponent<Rigidbody>().isKinematic = !active;
        if (!active)
        {
            GetComponent<UnitMovement>().stop(true);
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
