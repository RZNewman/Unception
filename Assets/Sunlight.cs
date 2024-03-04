using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sunlight : MonoBehaviour
{
    public Light daylight;
    public float daySpeedDegrees = 1;

    float lightMultiplier=1;

    enum LightCycle
    {
        Day,
        Night,
    }
    LightCycle cycle = LightCycle.Day;


    private void Update()
    {
        setCycle(Vector3.Angle(daylight.transform.forward, Vector3.down) < 96);
    }

    void setCycle(bool day)
    {
        LightCycle c = day ? LightCycle.Day : LightCycle.Night;

        if(cycle != c)
        {
            cycle = c;
            setLights();
        }
    }

    public void setMultiplier(float mult)
    {
        lightMultiplier = mult;
        setLights();
    }
    void setLights()
    {
        //daylight.enabled = cycle == LightCycle.Day;
        daylight.GetComponent<Spinner>().rotationSpeed = (cycle == LightCycle.Day ? daySpeedDegrees : daySpeedDegrees * 2) * lightMultiplier;
    }
}
