using UnityEngine;
using UnityEngine.UI;

public class UiAbility : MonoBehaviour
{
    public Image background;
    public Image foreground;
    Ability target;

    private void Update()
    {
        if (target)
        {
            bool fresh = target.cooldownMax == 0 || target.cooldownCurrent == 0;
            background.color = fresh ? Color.white : new Color(0.7f, 0.7f, 0.7f);
            foreground.fillAmount = fresh ? 0 : target.cooldownCurrent / target.cooldownMax;
        }

    }
    public void setTarget(Ability ability)
    {
        target = ability;
    }
}
