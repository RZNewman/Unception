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
    public Image critGameplay;


    public Image foreground;
    public Text chargeCount;



    Ability target;

    //menu only
    GroveWorld grove;
    string abilityID;
    Inventory inv;

    public enum UIAbilityMode
    {
        Game,
        Inventory
    }
    UIAbilityMode mode;


    public CastDataInstance blockFilled
    {
        get
        {
            if (target)
            {
                return (CastDataInstance)target.source();
            }
            else
            {
                return (CastDataInstance)inv.getAbilityInstance(abilityID);
            }
        }
    }

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
            critGameplay.gameObject.SetActive(target.willCrit && target.ready);
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

    public void setAbilityID(string id, Inventory i)
    {
        abilityID = id;
        inv = i;
        setFill((CastDataInstance)inv.getAbilityInstance(abilityID), UIAbilityMode.Inventory);
    }

    void setFill(CastDataInstance filled, UIAbilityMode m)
    {
        mode = m;
        AttackFlair flair = filled.flair;
        background.sprite = GlobalPrefab.gPre.bgFromQuality(filled.quality);
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
            keyDisplay.key = toKeyName(filled.slot.Value);
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
                Vector3 location = transform.position + (transform.right*slot.position.x + transform.up*slot.position.y) * 10 * shapeHolder.transform.lossyScale.x * GroveWorld.gridSpacing;
                Instantiate(ShapeLinkPre, location, transform.rotation, shapeHolder.transform).GetComponent<UIGroveLink>().setVisuals(flair, slot.type == GroveSlotType.Hard);
            }
        }




    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (mode == UIAbilityMode.Inventory)
        {
            grove.setHover(abilityID);
        }
        
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (mode == UIAbilityMode.Inventory)
        {
            grove.unsetHover(abilityID);
        }
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(mode == UIAbilityMode.Inventory)
        {
            if(eventData.button == PointerEventData.InputButton.Left)
            {
                Inventory inv = FindObjectOfType<GlobalPlayer>().player.GetComponent<Inventory>();
                grove.buildObject(abilityID);
                inv.CmdSendStorage(abilityID);
                grove.unsetHover(abilityID);
                Destroy(gameObject);
            }
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                Inventory inv = FindObjectOfType<GlobalPlayer>().player.GetComponent<Inventory>();
                inv.CmdSendTrash(abilityID);
                grove.unsetHover(abilityID);
                FindObjectOfType<GroveWorld>().sendIconToTrash(abilityID, gameObject);
            }

        }
        
    }

    

}
