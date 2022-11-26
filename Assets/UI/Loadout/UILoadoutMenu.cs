using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ItemList;

public class UILoadoutMenu : MonoBehaviour
{
    public List<GameObject> storageOnly;
    public List<GameObject> dropsOnly;

    public void loadInvMode(InventoryMode mode)
    {
        FindObjectOfType<ItemList>(true).fillAbilities(FindObjectOfType<GlobalPlayer>().player.GetComponent<Inventory>(), mode);

        switch (mode)
        {
            case InventoryMode.Drops:
                dropsOnly.ForEach(o => o.SetActive(true));
                storageOnly.ForEach(o => o.SetActive(false));
                break;
            case InventoryMode.Storage:
                dropsOnly.ForEach(o => o.SetActive(false));
                storageOnly.ForEach(o => o.SetActive(true));
                break;
        }
    }
}
