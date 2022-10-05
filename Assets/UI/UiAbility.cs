using UnityEngine;
using UnityEngine.UI;
using static GenerateAttack;
using static RewardManager;

public class UiAbility : MonoBehaviour
{
    public Image background;
    public Image symbol;
    public Image foreground;

    public Sprite Common;
    public Sprite Uncommon;
    public Sprite Rare;
    public Sprite Epic;
    public Sprite Legendary;
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
        AttackFlair flair = ability.source().flair;
        Texture2D texture = FindObjectOfType<Symbol>().symbols[flair.symbol];
        symbol.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        symbol.color = flair.color;
        background.sprite = bgFromQuality(ability.source().instance.quality);
    }

    Sprite bgFromQuality(Quality q)
    {
        Sprite bg;
        switch (q)
        {
            case Quality.Common:
                bg = Common;
                break;
            case Quality.Uncommon:
                bg = Uncommon;
                break;
            case Quality.Rare:
                bg = Rare;
                break;
            case Quality.Epic:
                bg = Epic;
                break;
            case Quality.Legendary:
                bg = Legendary;
                break;
            default:
                bg = Common;
                break;

        }
        return bg;
    }
}
