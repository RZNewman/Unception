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

    enum RegistrationState
    {
        None,
        Registered,
        AwaitStop
    }

    [SyncVar(hook =nameof(hookRegistration))]
    RegistrationState registered = RegistrationState.None;


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
        if (registered != RegistrationState.AwaitStop) yield break;

        globalUpdate.unregister(this);
        setUpdateScripts();

    }


    public void setRegistration(bool register, bool death = false)
    {
        RegistrationState target = (register, death) switch
        {
            (true, _) => RegistrationState.Registered,
            (_, true) => RegistrationState.None,
            _ => RegistrationState.AwaitStop,
        };
        if (target == registered) return;
        if (target == RegistrationState.AwaitStop && registered == RegistrationState.None) return;

        registered = target;
        if(target == RegistrationState.Registered && registered == RegistrationState.AwaitStop) return;
        setRegistrationHelper();
    }

    void hookRegistration(RegistrationState old, RegistrationState register)
    {
        if (isClientOnly)
        {
            if(register == RegistrationState.AwaitStop)
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
            switch (registered)
            {
                case RegistrationState.Registered:
                    globalUpdate.register(this);
                    setUpdateScripts();
                    break;
                case RegistrationState.None:
                    globalUpdate.unregister(this);
                    setUpdateScripts();
                    break;
                case RegistrationState.AwaitStop:
                    StartCoroutine(delayUnregister(globalUpdate));
                    break;
            }
            
        }
        
    }

    private void setUpdateScripts()
    {
        bool active = registered != RegistrationState.None;

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
