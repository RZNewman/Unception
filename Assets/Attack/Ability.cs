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
    public float cooldownCurrent = 0;

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

    void bufferClientCast(float old, float newCD)
    {
        //Debug.Log(newCD + " - " + old + ",   " + cooldownMax);
        if (newCD - old > cooldownMax * 0.8f)
        {
            clientCastBuffer++;
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
    public List<AttackSegment> cast(UnitMovement mover)
    {
        cooldownCurrent = cooldownMax;
        return attackFilled.buildStates(mover);
    }
    public void startCooldown()
    {
        cooldownTicking = true;
    }
    //static float clientCooldownThreshold = 0.001f;
    public bool ready
    {
        get
        {
            //Debug.Log(cooldownCurrent + " ##" + isClientOnly + " " + clientCastBuffer);
            //return isServer ? cooldownCurrent <= 0 : cooldownCurrent <= clientCooldownThreshold;
            bool offCD = cooldownCurrent <= 0;
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
