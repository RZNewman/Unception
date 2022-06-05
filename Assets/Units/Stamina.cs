using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stamina : NetworkBehaviour, BarValue
{
    [SyncVar]
    float maxStamina;
    [SyncVar]
    float currentStamina;

    float staminaRecover;

    public static float dashCost = 30;

    public float stamina
    {
        get
        {
            return currentStamina;
        }
    }
    public void spendStamina(float spent)
    {
        currentStamina -= spent;
    }

    void Start()
    {
        UnitProperties props = GetComponent<UnitPropsHolder>().props;
        maxStamina = props.maxStamina;
        staminaRecover = props.staminaRecover;


        currentStamina = maxStamina;
    }

    // Update is called once per frame
    public void ServerUpdate()
    {

        //change posture
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRecover * Time.fixedDeltaTime;
        }
        if (currentStamina > maxStamina)
        {
            currentStamina = maxStamina;
        }
    }

    public BarValue.BarData getBarFill()
    {
        return new BarValue.BarData
        {
            color = new Color(0.3f, 0.0f, 1.0f)/ (currentStamina>dashCost? 1.0f:2.0f),
            fillPercent = Mathf.Clamp01(currentStamina / maxStamina),
            active = true,
        };
    }

}
