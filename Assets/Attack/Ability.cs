using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AiHandler;

public class Ability : NetworkBehaviour
{

    [SyncVar]
    AttackBlock attackFormat;
    GameObject rotatingBody;

    public GameObject abilityIconPrefab;

    [SyncVar]
    public float cooldownCurrent=0;
    public bool cooldownTicking =false;
    // Start is called before the first frame update
    void Start()
    {
        rotatingBody = transform.parent.GetComponentInChildren<UnitRotation>().gameObject;
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
    public List<AttackState> cast()
	{
        cooldownCurrent = cooldownMax;
        return attackFormat.buildStates(this);
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
    public GameObject getSpawnBody()
    {
        return rotatingBody;
    }

    public EffectiveDistance GetEffectiveDistance()
    {
        return attackFormat.GetEffectiveDistance();
    }

}
