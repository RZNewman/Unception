using System.Collections.Generic;
using UnityEngine;


public class UnitUiReference : MonoBehaviour
{
    public UiBar healthbar;
    public UiBar staggerbar;
    public UiBar mezmerizeBar;
    public UiBar castbar;
    public UiBar staminaBar;
    public UiBar packHealBar;
    public UiText powerDisplayText;
    public UiBar powerDisplayBar;
    public UiBuffBar buffBar;
    public GameObject unitTarget;

    public List<GameObject> flatDisplays;

    private void Start()
    {
        foreach(GameObject d in flatDisplays)
        {
            d.SetActive(false);
        }
        if (unitTarget)
        {
            setSources();
        }

    }
    void setSources()
    {
        foreach (GameObject d in flatDisplays)
        {
            d.SetActive(true);
            UIKeyDisplay kdis = d.GetComponentInChildren<UIKeyDisplay>();
            if (kdis)
            {
                kdis.sync();
            }
        }
        healthbar.source = unitTarget.GetComponentInParent<Health>();
        staggerbar.source = unitTarget.GetComponentInParent<Posture>();
        castbar.source = unitTarget.GetComponentInParent<Cast>();
        if (mezmerizeBar)
        {
            mezmerizeBar.source = unitTarget.GetComponentInParent<Mezmerize>();
        }
        if (staminaBar)
        {
            staminaBar.source = unitTarget.GetComponentInParent<Stamina>();
        }
        if (powerDisplayText)
        {
            powerDisplayText.source = unitTarget.GetComponentInParent<Power>();
        }
        if (powerDisplayBar)
        {
            powerDisplayBar.source = unitTarget.GetComponentInParent<Power>();
        }
        if (packHealBar)
        {
            packHealBar.source = unitTarget.GetComponentInParent<PackHeal>();
        }
        if (buffBar)
        {
            unitTarget.GetComponentInParent<BuffManager>().subscribe(buffBar.displayBuffs);
        }
    }
    public void setTarget(GameObject t)
    {
        unitTarget = t;
        setSources();
    }
}
