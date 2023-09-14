using Mirror;
using UnityEngine;

public class Stamina : NetworkBehaviour, BarValue
{
    [SyncVar]
    float maxStamina;
    [SyncVar]
    float currentStamina;

    float staminaRecover;

    public static float dashCost = 30;

    Power power;
    UnitMovement mover;

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
        mover = GetComponent<UnitMovement>();
        power = GetComponent<Power>();
        maxStamina = props.maxStamina;
        staminaRecover = props.staminaRecover;


        currentStamina = maxStamina;
    }

    // Update is called once per frame
    public void OrderedUpdate()
    {
        float scaleTime = power.scaleTime();
        //float scaleNumerical = power.scaleNumerical();

        
        float staminaDelta = staminaRecover * Time.fixedDeltaTime * scaleTime;
        if(mover.legMode == UnitMovement.LegMode.Float)
        {
            if(currentStamina <= 0)
            {
                mover.legMode = UnitMovement.LegMode.Normal;
            }
            staminaDelta += -12  * Time.fixedDeltaTime * scaleTime;
        }


        currentStamina += staminaDelta;

        if (currentStamina > maxStamina)
        {
            currentStamina = maxStamina;
        }
    }

    public BarValue.BarData getBarFill()
    {
        return new BarValue.BarData
        {
            color = new Color(0.3f, 0.0f, 1.0f) / (currentStamina > dashCost ? 1.0f : 2.0f),
            fillPercent = Mathf.Clamp01(currentStamina / maxStamina),
            active = true,
        };
    }

}
