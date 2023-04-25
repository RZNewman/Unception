using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static GenerateAttack;
using static UnitControl;

public class ItemList : MonoBehaviour, UiDraggerTarget, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject abilityIconPre;

    UiEquipmentDragger dragger;

    Inventory inv;
    public InventoryMode mode;
    GlobalPlayer gp;

    private void Start()
    {
        gp = FindObjectOfType<GlobalPlayer>(true);
        dragger = FindObjectOfType<UiEquipmentDragger>(true);
    }
    public enum InventoryMode
    {
        Storage,
        Drops,
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        dragger.setTarget(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        dragger.unsetTarget(this);
    }
    public void setInventory(Inventory i)
    {
        inv = i;
    }

    public void fillAbilities()
    {




        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }


        List<AttackBlock> source;
        switch (mode)
        {
            case InventoryMode.Drops:
                source = inv.dropped;
                break;
            case InventoryMode.Storage:
            default:
                source = inv.stored;
                break;
        }

        source.ForEach(a => createIcon(a));
        sort();

        displayUpgrades();
    }

    public void displayUpgrades()
    {
        foreach (Transform icon in transform)
        {
            UiAbility uia = icon.GetComponent<UiAbility>();
            uia.setUpgrade(dragger.GetComponent<UILoadoutMenu>().slotList.slotOfType(uia.blockFilled.slot.Value).actingPower < uia.blockFilled.instance.actingPower);
        }
    }
    public GameObject createIcon(AttackBlock ability)
    {
        GameObject icon = Instantiate(abilityIconPre, transform);
        icon.transform.localScale = Vector3.one * 0.6f;
        UiAbility uia = icon.GetComponent<UiAbility>();
        uia.setFill(inv.fillBlock(ability));
        uia.setDragger(dragger);
        return icon;
    }
    void sort()
    {
        transform.SortChildren(sortFunction(), true);
    }
    public enum SortMode
    {
        ActingPower,
        Cooldown,
        CastTime,
    }
    public SortMode sortMode = SortMode.ActingPower;

    public void setSortMode(SortModeID id)
    {
        sortMode = id.mode;
        sort();
    }

    public System.Func<Transform, System.IComparable> sortFunction()
    {
        switch (sortMode)
        {
            case SortMode.Cooldown:
                return (t1) => t1.GetComponent<UiAbility>().ability.getCooldownDisplay(gp.player.power);
            case SortMode.CastTime:
                return (t1) => t1.GetComponent<UiAbility>().ability.instance.castTimeDisplay(gp.player.power);
            case SortMode.ActingPower:
            default:
                return (t1) => t1.GetComponent<UiAbility>().ability.instance.actingPower;

        }
    }


    public void grabAbility(GameObject icon)
    {

        icon.transform.SetParent(transform);
        sort();
    }

    public void slotObject(GameObject uiAbil)
    {
        UiAbility newUI = uiAbil.GetComponent<UiAbility>();
        grabAbility(uiAbil);
        switch (mode)
        {
            case InventoryMode.Storage:
                gp.player.GetComponent<Inventory>().CmdSendStorage(newUI.blockFilled.id);
                break;
            case InventoryMode.Drops:
                gp.player.GetComponent<Inventory>().CmdSendTrash(newUI.blockFilled.id);
                break;
        }
    }
}
