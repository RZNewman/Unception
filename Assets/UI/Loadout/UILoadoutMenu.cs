using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GenerateAttack;
using static ItemList;

public class UILoadoutMenu : MonoBehaviour
{
    public ItemList storageList;
    public ItemList dropsList;
    public UiSlotList slotList;

    public void loadInvMode()
    {
        slotList.clear();
        UiEquipmentDragger dragger = GetComponent<UiEquipmentDragger>();
        Inventory inv = FindObjectOfType<GlobalPlayer>().player.GetComponent<Inventory>();
        storageList.setInventory(inv);
        dropsList.setInventory(inv);

        List<(ItemSlot, GameObject)> iconList = inv.equippedAbilities.Select(pair => (pair.Key, storageList.createIcon(pair.Value))).ToList();
        Dictionary<ItemSlot, GameObject> icons = new Dictionary<ItemSlot, GameObject>();
        iconList.ForEach((item) => icons.Add(item.Item1, item.Item2));
        slotList.fillSlots(icons, dragger);

        storageList.fillAbilities();
        dropsList.fillAbilities();

    }

    public void displayUpgrades()
    {
        storageList.displayUpgrades();
        dropsList.displayUpgrades();
    }
}
