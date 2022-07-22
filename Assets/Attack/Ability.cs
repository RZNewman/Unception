using Mirror;
using System.Collections.Generic;
using UnityEngine;
using static AiHandler;

public class Ability : NetworkBehaviour
{

    [SyncVar]
    AttackBlock attackFormat;

    AttackBlockFilled attackFilled;

    public GameObject abilityIconPrefab;

    [SyncVar]
    public float cooldownCurrent = 0;
    public bool cooldownTicking = false;
    // Start is called before the first frame update
    void Start()
    {
        if (GetComponentInParent<LocalPlayer>().isLocalPlayer)
        {
            GameObject bar = GameObject.FindGameObjectWithTag("LocalAbilityBar");
            GameObject icon = Instantiate(abilityIconPrefab, bar.transform);
            icon.GetComponent<UiAbility>().setTarget(this);
        }
        if (isClientOnly)
        {
            fillFormat();
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (cooldownCurrent > 0 && cooldownTicking)
        {
            cooldownCurrent -= Time.fixedDeltaTime;
        }
        if (cooldownCurrent <= 0)
        {
            cooldownTicking = false;
            cooldownCurrent = 0;
        }
    }
    public float cooldownMax
    {
        get
        {
            return attackFilled.getCooldown();
        }
    }
    public List<PlayerMovementState> cast(UnitMovement mover)
    {
        cooldownCurrent = cooldownMax;
        return attackFilled.buildStates(mover);
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
        fillFormat();
        if (attackFormat.scales)
        {
            GetComponentInParent<Power>().subscribePower(scaleAbility);
        }
    }
    void fillFormat()
    {
        attackFilled = GenerateAttack.fillBlock(attackFormat);
    }
    void scaleAbility(Power p)
    {
        attackFilled = GenerateAttack.fillBlock(attackFormat, p.power);
    }


    public EffectiveDistance GetEffectiveDistance()
    {
        return attackFilled.GetEffectiveDistance();
    }

}
