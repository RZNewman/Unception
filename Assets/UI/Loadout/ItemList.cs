using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static GenerateAttack;
using static UnitControl;

public class ItemList : MonoBehaviour
{
    public GameObject abilityIconPre;

    UiEquipmentDragger drag;
    UiAbilityDetails deets;
    UiSlotList slotList;
    Inventory inv;
    GlobalPlayer gp;

    private void Start()
    {
        gp = FindObjectOfType<GlobalPlayer>(true);
    }
    public enum InventoryMode
    {
        Storage,
        Drops,
    }
    public void fillAbilities(Inventory i, InventoryMode mode)
    {
        inv = i;
        deets = FindObjectOfType<UiAbilityDetails>(true);
        drag = FindObjectOfType<UiEquipmentDragger>(true);
        slotList = FindObjectOfType<UiSlotList>(true);

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        slotList.clear();

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
        List<(ItemSlot, GameObject)> iconList = inv.equippedAbilities.Select(pair => (pair.Key, createIcon(pair.Value))).ToList();
        Dictionary<ItemSlot, GameObject> icons = new Dictionary<ItemSlot, GameObject>();
        iconList.ForEach((item) => icons.Add(item.Item1, item.Item2));
        slotList.fillSlots(icons, drag, this, mode);
    }
    GameObject createIcon(AttackBlock ability)
    {
        GameObject icon = Instantiate(abilityIconPre, transform);
        UiAbility uia = icon.GetComponent<UiAbility>();
        uia.setFill(inv.fillBlock(ability));
        uia.setDetails(deets);
        uia.setDragger(drag);
        uia.inventoryIndex = ability.id;
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
        UiAbility uia = icon.GetComponent<UiAbility>();
        uia.setDragger(drag);
        uia.setSlot(null);
        sort();
    }
}
