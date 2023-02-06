using UnityEngine;


public class UnitUiReference : MonoBehaviour
{
    public UiBar healthbar;
    public UiBar staggerbar;
    public UiBar castbar;
    public UiBar staminaBar;
    public UiBar packHealBar;
    public UiText powerDisplay;
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
    }
    public void setTarget(GameObject t)
    {
        unitTarget = t;
        setSources();
    }
}
