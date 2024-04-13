using Mirror;
using System.Collections.Generic;
using UnityEngine;
using static AiHandler;
using static AttackMachine;
using static AttackSegment;
using static AttackUtils;
using static GenerateAttack;
using static Power;
using static Size;
using static StatTypes;
using static UnitControl;
using static Utils;

public class Ability : NetworkBehaviour
{
    StatHandler statHandler;

    [SyncVar]
    AbilityData format;


    [SyncVar]
    public ItemSlot? clientSyncKey;

    AbilityDataInstance formatInstance;

    public GameObject abilityIconPrefab;
    GameObject icon;

    [SyncVar(hook = nameof(bufferClientCast))]
    public float charges = 1;

    uint clientCastBuffer = 0;

    [SyncVar]
    public bool cooldownTicking = false;

    [SyncVar]
    int criticalCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<ClientAdoption>().trySetAdopted();
        LocalPlayer p = GetComponentInParent<LocalPlayer>();
        Power power = GetComponentInParent<Power>();
        if (isClientOnly)
        {
            fillFormat(power);
            if (clientSyncKey.HasValue)
            {
                GetComponentInParent<AbilityManager>().registerAbility(clientSyncKey.Value, this);
            }

        }
        if (p.isClient && p.isOwned && clientSyncKey.HasValue)
        {
            GameObject bar = GameObject.FindGameObjectWithTag("LocalAbilityBar");
            icon = Instantiate(abilityIconPrefab, bar.transform);
            icon.GetComponent<UiAbility>().setTarget(this);
            bar.transform.SortChildren(c => c.GetComponent<UiAbility>().blockFilled.slot);
        }

        if (isServer)
        {
            transform.parent.GetComponent<StatHandler>().link(GetComponent<StatHandler>());
        }

    }

    public bool isHardcast
    {
        get
        {
            return slot().HasValue;
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
            return formatInstance.flair.name;
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
     public void Tick()
    {
        if (charges < chargeMax && cooldownTicking)
        {
            charges += Time.fixedDeltaTime * formatInstance.effect.getCooldownMult() / cooldownPerCharge;
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
            return formatInstance.effect.cooldown;
        }
    }

    float chargeMax=0;
    float chargeMaxCalculated
    {
        get
        {
            return formatInstance.effect.getCharges();
        }
    }

    bool canCrit
    {
        get
        {
            return formatInstance.effect.criticalCount > 0;
        }
    }
    public bool willCrit
    {
        get
        {
            return formatInstance.effect.criticalCount == criticalCount + 2;
        }
    }
    bool isCrit
    {
        get
        {
            return formatInstance.effect.criticalCount == criticalCount + 1;
        }
    }

    float critEffect
    {
        get
        {
            return 1 + (isCrit ? formatInstance.effect.criticalCount * formatInstance.effect.criticalModifier : -formatInstance.effect.criticalModifier);
        }
    }

    void trySetCritScaling()
    {
        if(canCrit)
        {
            formatInstance.tempStrengthEffect = new StrengthMultiplers(0, critEffect);
        }
    }
    public List<AttackSegment> cast(UnitMovement mover, CastingLocationData castData)
    {
        if (canCrit)
        {
            criticalCount = (criticalCount+1) % formatInstance.effect.criticalCount;
            trySetCritScaling();
        }
        if (cooldownPerCharge > 0)
        {
            charges -= 1;
            cooldownTicking = false;
        }
        return AttackSegment.buildStates(formatInstance.effect, mover, castData);
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
    public void setFormat(AbilityData a)
    {
        format = a;      
        GetComponentInParent<Power>().subscribePower(fillFormat);
        charges = chargeMax;
    }
    BaseScales cachedBaseScales;

    void fillFormat(Power p)
    {
        if(format.scales || !cachedBaseScales.Equals(p.instanceBaseScales))
        {
            cachedBaseScales = p.instanceBaseScales;
            //Debug.Log(cachedBaseScales.world + " - " + cachedBaseScales.time);
            formatInstance = format.populate(
                new FillBlockOptions
                {
                    statLinkAbility = this,
                    overridePower = p.power,
                    baseScales = cachedBaseScales,


                }
                );
            chargeMax = chargeMaxCalculated;
            trySetCritScaling();
        }
        

    }

    void setCastStrength(float percent)
    {
        formatInstance.tempStrengthEffect = new StrengthMultiplers(0, percent);
    }



    public EffectiveDistance GetEffectiveDistance(CapsuleSize sizeC)
    {
        return formatInstance.effect.GetEffectiveDistance(sizeC);
    }

    public ItemSlot? slot()
    {
        return formatInstance switch
        {
            CastDataInstance c => c.slot,
            _ => null
        };
    }
    public AbilityDataInstance source()
    {
        return formatInstance;
    }

}
