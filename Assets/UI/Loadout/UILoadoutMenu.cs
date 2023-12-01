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

    public void loadInvMode()
    {
        UiEquipmentDragger dragger = GetComponent<UiEquipmentDragger>();
        PlayerGhost player = FindObjectOfType<GlobalPlayer>().player;
        Grove grove = player.GetComponent<Grove>();
        Inventory inv = player.GetComponent<Inventory>();
        storageList.setInventory(inv);
        dropsList.setInventory(inv);

        GroveWorld groveW = FindObjectOfType<GroveWorld>(true);
        groveW.reset();
        grove.exportPlacements().ToList().ForEach(pair => groveW.buildPlacedObject(pair.Key, pair.Value));

        storageList.fillAbilities();
        dropsList.fillAbilities();

    }

    public void returnObject(string id)
    {
        storageList.createIcon(id);
    }

    public void trashObject(string id)
    {
        dropsList.createIcon(id);
    }

    public void displayUpgrades()
    {
        storageList.displayUpgrades();
        dropsList.displayUpgrades();
    }
}
