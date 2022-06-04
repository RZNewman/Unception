using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitUiReference : MonoBehaviour
{
    public UiBar healthbar;
    public UiBar staggerbar;
    public UiBar castbar;
    public GameObject unitTarget;

    private void Start()
    {
        if (unitTarget)
        {
            setSources();
        }
        
    }
    void setSources()
    {
        healthbar.source = unitTarget.GetComponentInParent<Health>();
        staggerbar.source = unitTarget.GetComponentInParent<Posture>();
        castbar.source = unitTarget.GetComponentInParent<Cast>();
    }
    public void setTarget(GameObject t)
    {
        unitTarget = t;
        setSources();
    }
}
