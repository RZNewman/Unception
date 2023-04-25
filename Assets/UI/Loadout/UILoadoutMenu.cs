using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ItemList;

public class UILoadoutMenu : MonoBehaviour
{
    public ItemList storageList;
    public ItemList dropsList;

    public void loadInvMode()
    {
        Inventory inv = FindObjectOfType<GlobalPlayer>().player.GetComponent<Inventory>();
        storageList.fillAbilities(inv, InventoryMode.Storage);
        dropsList.fillAbilities(inv, InventoryMode.Drops);


    }
}
