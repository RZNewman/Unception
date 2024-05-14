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

    [SyncVar(hook =nameof(hookRegistration))]
    bool registered;


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

        setUpdateScripts(registered);
    }

    public void logUnit()
    {
        Debug.Log(name);
    }

    IEnumerator delayUnregister(DeterministicUpdate globalUpdate)
    {
        while (!life.IsDead || !mover.grounded)
        {
            yield return null;
        }
        globalUpdate.unregister(this);
        registered =false;
        setUpdateScripts(registered);

    }


    public void setRegistration(bool register, bool death = false)
    {
        if (register == registered) return;

        registered = register;
        setRegistrationHelper(register, death);
    }

    void hookRegistration(bool old, bool register)
    {
        if (isClientOnly)
        {
            setRegistrationHelper(register, true);
        }
    }

    void setRegistrationHelper(bool register, bool noWait)
    {
        globalUpdate = FindObjectOfType<DeterministicUpdate>(true);
        if (globalUpdate)
        {
            if (register)
            {
                globalUpdate.register(this);
                setUpdateScripts(register);
            }
            else
            {
                if (noWait)
                {
                    globalUpdate.unregister(this);
                    setUpdateScripts(register);
                }
                else
                {
                    StartCoroutine(delayUnregister(globalUpdate));
                }
                
            }
            
        }
        
    }

    private void setUpdateScripts(bool active)
    {
        GetComponent<ControlManager>().enabled = active;
        GetComponentInChildren<UnitRotation>().enabled = active;
        GetComponentInChildren<UnitEye>().enabled = active;
        GetComponent<Rigidbody>().isKinematic = !active;
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
