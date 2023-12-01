using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static GenerateAttack;
using static UnitControl;

public class ItemList : MonoBehaviour, UiDraggerTarget
{
    public GameObject abilityIconPre;



    Inventory inv;
    public InventoryMode mode;
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


        List<CastData> source;
        switch (mode)
        {
            case InventoryMode.Drops:
                source = inv.dropped;
                break;
            case InventoryMode.Storage:
            default:
                source = inv.unequipped;
                break;
        }

        source.ForEach(a => createIcon(a.id));
        sort();

        displayUpgrades();
    }

    public void displayUpgrades()
    {
        foreach (Transform icon in transform)
        {
            UiAbility uia = icon.GetComponent<UiAbility>();
            float slotPower = FindObjectOfType<Grove>().powerOfSlot(uia.blockFilled.slot.Value);
            float abPower = uia.blockFilled.actingPower();
            uia.setUpgrade(slotPower < abPower);
        }
    }

    public GameObject createIcon(string id)
    {
        GameObject icon = Instantiate(abilityIconPre, transform);
        icon.transform.localScale = Vector3.one * 0.6f;
        UiAbility uia = icon.GetComponent<UiAbility>();
        uia.setAbilityID(id, inv);
        return icon;
    }


    void sort()
    {
        transform.SortChildren(sortFunction(), !reverse);
    }
    public enum SortMode
    {
        ActingPower,
        Cooldown,
        CastTime,
        DPS,
    }
    public SortMode sortMode = SortMode.ActingPower;
    bool reverse = false;
    public void setSortMode(SortModeID id)
    {
        if (sortMode == id.mode)
        {
            reverse = !reverse;
        }
        else
        {
            reverse = false;
        }
        sortMode = id.mode;

        sort();
    }

    public System.Func<Transform, System.IComparable> sortFunction()
    {
        switch (sortMode)
        {
            case SortMode.DPS:
                return (t1) => t1.GetComponent<UiAbility>().blockFilled.effect.dps(gp.player.power);
            case SortMode.Cooldown:
                return (t1) => t1.GetComponent<UiAbility>().blockFilled.effect.cooldownDisplay(gp.player.power);
            case SortMode.CastTime:
                return (t1) => t1.GetComponent<UiAbility>().blockFilled.effect.castTimeDisplay(gp.player.power);
            case SortMode.ActingPower:
            default:
                return (t1) => t1.GetComponent<UiAbility>().blockFilled.actingPower();

        }
    }


    public void grabAbility(GameObject icon)
    {

        //icon.transform.SetParent(transform);
        //sort();
    }

    public void slotObject(GameObject uiAbil)
    {
        //UiAbility newUI = uiAbil.GetComponent<UiAbility>();
        //grabAbility(uiAbil);
        //switch (mode)
        //{
        //    case InventoryMode.Storage:
        //        gp.player.GetComponent<Inventory>().CmdSendStorage(newUI.blockFilled.id);
        //        break;
        //    case InventoryMode.Drops:
        //        gp.player.GetComponent<Inventory>().CmdSendTrash(newUI.blockFilled.id);
        //        break;
        //}
    }
}
