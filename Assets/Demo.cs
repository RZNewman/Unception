using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void scaleItems()
    {
        GetComponent<GlobalPlayer>().serverPlayer.unit.GetComponent<AbiltyManager>().allAbilities().ForEach(a => a.demoForceScale());
    }

    public void PowerTrickle()
    {
        Power p =GetComponent<GlobalPlayer>().serverPlayer.unit.GetComponent<Power>();
        StartCoroutine(periodicPower(p));
    }

    IEnumerator periodicPower( Power p)
    {
        float powerAmount = p.power *1.5f;
        int segments = 60;
        float powerPer = powerAmount / segments;
        for (int i = 0; i < segments; i++)
        {
            p.addPower(powerPer);
            yield return new WaitForSeconds(1.0f);
        }
    }
}
