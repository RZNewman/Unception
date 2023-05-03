using UnityEngine;


public class UnitUiReference : MonoBehaviour
{
    public UiBar healthbar;
    public UiBar staggerbar;
    public UiBar staggermirror;
    public UiBar castbar;
    public UiBar staminaBar;
    public UiBar packHealBar;
    public UiText powerDisplay;
    public UiBuffBar buffBar;
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
        if (staggermirror)
        {
            staggermirror.source = unitTarget.GetComponentInParent<Posture>();
        }
        if (staminaBar)
        {
            staminaBar.source = unitTarget.GetComponentInParent<Stamina>();
        }
        if (powerDisplay)
        {
            powerDisplay.source = unitTarget.GetComponentInParent<Power>();
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
