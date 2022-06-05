using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AiHandler;

public class Ability : NetworkBehaviour
{

    [SyncVar]
    AttackBlock attackFormat;

    public GameObject abilityIconPrefab;

    [SyncVar]
    public float cooldownCurrent=0;
    public bool cooldownTicking =false;
    // Start is called before the first frame update
    void Start()
    {
        if (GetComponentInParent<LocalPlayer>().isLocalPlayer)
        {
            GameObject bar = GameObject.FindGameObjectWithTag("LocalAbilityBar");
            GameObject icon = Instantiate(abilityIconPrefab, bar.transform);
            icon.GetComponent<UiAbility>().setTarget(this);
        }
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (cooldownCurrent > 0 && cooldownTicking)
        {
            cooldownCurrent-= Time.fixedDeltaTime;
        }
        if(cooldownCurrent<= 0)
        {
            cooldownTicking = false;
            cooldownCurrent = 0;
        }
    }
    public float cooldownMax
    {
        get
        {
            return attackFormat.getCooldown();
        }
    }
    public List<PlayerMovementState> cast(UnitMovement mover)
	{
        cooldownCurrent = cooldownMax;
        return attackFormat.buildStates(mover);
    }
    public void startCooldown()
    {
        cooldownTicking = true;
    }
    public bool ready
    {
        get
        {
            return cooldownCurrent <= 0;
        }
    }
    public void setFormat(AttackBlock b)
    {
        attackFormat = b;
    }
    

    public EffectiveDistance GetEffectiveDistance()
    {
        return attackFormat.GetEffectiveDistance();
    }

}
