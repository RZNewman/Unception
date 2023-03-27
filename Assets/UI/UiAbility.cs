using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static GenerateAttack;
using static RewardManager;

public class UiAbility : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image background;
    public Text identifier;
    public Image symbol;
    public Image foreground;
    public Text chargeCount;

    public Sprite Common;
    public Sprite Uncommon;
    public Sprite Rare;
    public Sprite Epic;
    public Sprite Legendary;

    Ability target;
    AttackBlockFilled filled;

    //menu only
    UiAbilityDetails deets;
    UiEquipmentDragger dragger;
    UiEquipSlot slot;
    public string inventoryIndex;

    private void Update()
    {
        if (target)
        {
            bool fresh = target.charges >= 1;
            float charges = target.charges;
            background.color = fresh ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            foreground.fillAmount = charges == 1 ? 0 : 1 - (charges % 1);
            chargeCount.text = charges > 1 ? Mathf.Floor(charges).ToString() : "";
        }

    }
    public void setTarget(Ability ability)
    {
        target = ability;
        setFill(ability.source());
    }

    public void setFill(AttackBlockFilled a)
    {
        filled = a;
        AttackFlair flair = filled.flair;
        Texture2D texture = FindObjectOfType<Symbol>().symbols[flair.symbol];
        symbol.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        symbol.color = flair.color;
        Color partialColor = flair.color;
        partialColor.a = 0.4f;
        identifier.color = partialColor;
        identifier.text = flair.identifier;
        background.sprite = bgFromQuality(filled.instance.quality);
        GetComponentInChildren<StarCounter>().setStars(filled.instance.mods != null ? filled.instance.mods.Length : 0);
    }

    public void setDetails(UiAbilityDetails details)
    {
        deets = details;

    }
    public void setDragger(UiEquipmentDragger drag)
    {
        dragger = drag;
    }

    public void setSlot(UiEquipSlot s)
    {
        slot = s;
    }

    public void takeFromSlot()
    {
        if (slot)
        {
            slot.unslot();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (deets)
        {
            deets.setDetails(filled);
        }
        if (dragger)
        {
            dragger.setHover(this);
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (dragger)
        {
            dragger.unsetHover(this);
        }
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

    public AttackBlockFilled ability
    {
        get
        {
            return filled;
        }
    }

}
