using Mirror;
using System.Collections.Generic;
using UnityEngine;
using static AiHandler;
using static AttackMachine;
using static AttackSegment;
using static AttackUtils;
using static GenerateAttack;
using static StatTypes;
using static UnitControl;
using static Utils;

public class Ability : NetworkBehaviour
{
    StatHandler statHandler;

    [SyncVar]
    AttackBlock attackFormat;

    [SyncVar]
    float? triggerStrength;

    [SyncVar]
    public ItemSlot? clientSyncKey;

    AttackBlockInstance attackFilled;

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
        if (isClientOnly)
        {
            fillFormat();
            if (clientSyncKey.HasValue)
            {
                GetComponentInParent<AbiltyManager>().registerAbility(clientSyncKey.Value, this);
            }

        }
        if (p.isClient && p.hasAuthority && clientSyncKey.HasValue)
        {
            GameObject bar = GameObject.FindGameObjectWithTag("LocalAbilityBar");
            icon = Instantiate(abilityIconPrefab, bar.transform);
            icon.GetComponent<UiAbility>().setTarget(this);
            bar.transform.SortChildren(c => c.GetComponent<UiAbility>().blockFilled.slot);
        }

        if (isServer)
        {
            //TODO once hardcast is baken into the ability, prevent triggers from gaining stats
            transform.parent.GetComponent<StatHandler>().link(GetComponent<StatHandler>());
        }

    }
    private void OnDestroy()
    {
        if (icon)
        {
            Destroy(icon);
        }
    }
    public string abilityName
    {
        get
        {
            return attackFilled.flair.name;
        }
    }

    void bufferClientCast(float old, float newcharge)
    {
        //Debug.Log(newCD + " - " + old + ",   " + cooldownMax);
        if (old - newcharge > 0.8f)
        {
            clientCastBuffer++;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (charges < chargeMax && cooldownTicking)
        {
            charges += Time.fixedDeltaTime * attackFilled.instance.getCooldownMult() / cooldownPerCharge;
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
            return attackFilled.instance.cooldown;
        }
    }

    float chargeMax
    {
        get
        {
            return attackFilled.instance.getCharges();
        }
    }
    public List<AttackSegment> cast(UnitMovement mover, CastingLocationData castData)
    {
        if (cooldownPerCharge > 0)
        {
            charges -= 1;
            cooldownTicking = false;
        }
        return AttackSegment.buildStates(attackFilled.instance, mover, castData);
    }
    public void startCooldown()
    {
        if (cooldownPerCharge > 0)
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
    public void setFormat(AttackBlock b, float? tStrength = null)
    {
        attackFormat = b;
        triggerStrength = tStrength;
        fillFormat();
        charges = chargeMax;
        if (attackFormat.scales)
        {
            subscribeScale();
        }
    }
    void fillFormat()
    {
        attackFilled = attackFormat.fillBlock(
            new AttackBlock.FillBlockOptions
            {
                statLinkAbility = this,
                addedStrength = triggerStrength,
                reduceWindValue = triggerStrength.HasValue
            }
            );

    }

    public void demoForceScale()
    {
        GetComponentInParent<Power>().subscribePower(p =>
        {
            attackFilled = attackFormat.fillBlock(
            new AttackBlock.FillBlockOptions
            {
                statLinkAbility = this,
                addedStrength = triggerStrength,
                reduceWindValue = triggerStrength.HasValue,
                overridePower = p.power,
                forceScaling = true,
            }
            );
        });
    }
    void subscribeScale()
    {
        GetComponentInParent<Power>().subscribePower(scaleAbility);
    }
    void scaleAbility(Power p)
    {
        attackFilled = attackFormat.fillBlock(
            new AttackBlock.FillBlockOptions
            {
                statLinkAbility = this,
                addedStrength = triggerStrength,
                reduceWindValue = triggerStrength.HasValue,
                overridePower = p.power
            }
            );
    }


    public EffectiveDistance GetEffectiveDistance(float halfHeight)
    {
        return attackFilled.instance.GetEffectiveDistance(halfHeight);
    }

    public AttackBlockInstance source()
    {
        return attackFilled;
    }

}
