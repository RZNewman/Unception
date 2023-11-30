using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static GenerateAttack;
using static GroveObject;
using static RewardManager;
using static UnitControl;

public class UiAbility : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image background;

    public GameObject ShapeLinkPre;
    public GameObject shapeHolder;

    public GameObject gameplayView;
    public GameObject invView;

    public Text identifierGamplay;
    public Image symbolGameplay;
    public Text identifierInv;
    public Image symbolInv;
    public Image slotInv;
    public UiBuffBar buffBarGameplay;


    public Image foreground;
    public Text chargeCount;

    public Sprite Common;
    public Sprite Uncommon;
    public Sprite Rare;
    public Sprite Epic;
    public Sprite Legendary;

    Ability target;
    CastDataInstance filled;

    //menu only
    GroveWorld grove;

    public enum UIAbilityMode
    {
        Game,
        Inventory
    }
    UIAbilityMode mode;


    private void Start()
    {
        grove = FindObjectOfType<GroveWorld>();
    }
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

    public void setUpgrade(bool upgrade)
    {
        background.color = upgrade ? Color.white : new Color(0.5f, 0.5f, 0.5f);
    }
    public void setTarget(Ability ability)
    {
        target = ability;
        setFill((CastDataInstance)ability.source(), UIAbilityMode.Game);
    }
    public CastDataInstance blockFilled
    {
        get
        {
            return filled;
        }
    }

    public void setFill(CastDataInstance a, UIAbilityMode m)
    {
        filled = a;
        mode = m;
        AttackFlair flair = filled.flair;
        background.sprite = bgFromQuality(filled.quality);
        Symbol symbolSource = FindObjectOfType<Symbol>();
        Color partialColor = flair.color;
        partialColor.a = 0.4f;
        if (mode == UIAbilityMode.Game)
        {
            gameplayView.SetActive(true);
            invView.SetActive(false);
            symbolGameplay.sprite = symbolSource.symbols[flair.symbol];
            symbolGameplay.color = flair.color;
            identifierGamplay.color = partialColor;
            identifierGamplay.text = flair.identifier;
            UIKeyDisplay keyDisplay = GetComponentInChildren<UIKeyDisplay>();
            keyDisplay.key = toKeyName(a.slot.Value);
            keyDisplay.sync();
            target.GetComponent<BuffManager>().subscribe(buffBarGameplay.displayBuffs);

        }
        else
        {
            gameplayView.SetActive(false);
            invView.SetActive(true);
            GetComponentInChildren<StarCounter>().setStars(filled.stars);
            symbolInv.sprite = symbolSource.symbols[flair.symbol];
            symbolInv.color = flair.color;
            identifierInv.color = partialColor;
            identifierInv.text = flair.identifier;
            slotInv.sprite = symbolSource.fromSlot(filled.slot ?? ItemSlot.Main);

            foreach (GroveSlotPosition slot in filled.shape.points)
            {
                Vector3 location = transform.position + new Vector3(slot.position.x, slot.position.y) * 10 * shapeHolder.transform.lossyScale.x * GroveWorld.gridSpacing;
                Instantiate(ShapeLinkPre, location, Quaternion.identity, shapeHolder.transform).GetComponent<UIGroveLink>().setVisuals(flair.color, slot.type == GroveSlotType.Hard);
            }
        }




    }


    public void OnPointerEnter(PointerEventData eventData)
    {
         grove.setHover(this);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        grove.unsetHover(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(mode == UIAbilityMode.Inventory)
        {
            FindObjectOfType<GroveWorld>().buildObject(filled);
            grove.unsetHover(this);
            Destroy(gameObject);
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

    

    public CastDataInstance ability
    {
        get
        {
            return filled;
        }
    }

}
