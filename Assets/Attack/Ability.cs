using Mirror;
using System.Collections.Generic;
using UnityEngine;
using static AiHandler;
using static AttackUtils;
using static UnitControl;

public class Ability : NetworkBehaviour
{

    [SyncVar]
    AttackBlock attackFormat;

    [SyncVar]
    public AttackKey clientSyncKey;

    AttackBlockFilled attackFilled;

    public GameObject abilityIconPrefab;
    GameObject icon;

    [SyncVar(hook = nameof(bufferClientCast))]
    public float charges = 1;

    uint clientCastBuffer = 0;

    [SyncVar]
    public bool cooldownTicking = false;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<ClientAdoption>().trySetAdopted();
        LocalPlayer p = GetComponentInParent<LocalPlayer>();
        if (p.isClient && p.hasAuthority)
        {
            GameObject bar = GameObject.FindGameObjectWithTag("LocalAbilityBar");
            icon = Instantiate(abilityIconPrefab, bar.transform);
            icon.GetComponent<UiAbility>().setTarget(this);
        }
        if (isClientOnly)
        {
            fillFormat();
            GetComponentInParent<AbiltyList>().registerAbility(clientSyncKey, this);
        }

    }
    private void OnDestroy()
    {
        if (icon)
        {
            Destroy(icon);
        }
    }

    void bufferClientCast(float old, float newcharge)
    {
        //Debug.Log(newCD + " - " + old + ",   " + cooldownMax);
        if (old - newcharge  >  0.8f)
        {
            clientCastBuffer++;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (charges < chargeMax && cooldownTicking)
        {
            charges += Time.fixedDeltaTime/cooldownPerCharge;
        }
        if (charges >= chargeMax)
        {
            cooldownTicking = false;
            charges = chargeMax;
        }
    }

    public float cooldownPerCharge
    {
        get
        {
            return attackFilled.getCooldown();
        }
    }

    float chargeMax
    {
        get
        {
            return attackFilled.getCharges();
        }
    }
    public List<AttackSegment> cast(UnitMovement mover)
    {
        if (cooldownPerCharge > 0)
        {
            charges -= 1;
            cooldownTicking = false;
        }
        return attackFilled.buildStates(mover);
    }
    public void startCooldown()
    {
        if(cooldownPerCharge > 0)
        {
            cooldownTicking = true;
        }
        
    }
    //static float clientCooldownThreshold = 0.001f;
    public bool ready
    {
        get
        {
            //Debug.Log(cooldownCurrent + " ##" + isClientOnly + " " + clientCastBuffer);
            //return isServer ? cooldownCurrent <= 0 : cooldownCurrent <= clientCooldownThreshold;
            bool offCD = charges >= 1;
            if (!offCD)
            {
                if (isClientOnly && clientCastBuffer > 0)
                {
                    clientCastBuffer--;
                    offCD = true;
                }
            }
            return offCD;
        }
    }
    [Server]
    public void setFormat(AttackBlock b)
    {
        attackFormat = b;
        fillFormat();
        charges = chargeMax;
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

    public AttackBlockFilled source()
    {
        return attackFilled;
    }

}
