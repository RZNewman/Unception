using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ItemList;

public class UiEquipSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UiEquipmentDragger dragger;
    public ItemList itemTray;
    public InventoryMode invMode;

    GameObject uiAbility;

    public enum SlotMode
    {
        Equipment,
        Trash,
        Store,
    }
    public SlotMode mode = SlotMode.Equipment;

    GlobalPlayer gp;
    private void Start()
    {
        gp = FindObjectOfType<GlobalPlayer>(true);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        dragger.setSlot(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        dragger.unsetSlot(this);
    }

    public void slotObject(GameObject uiAbil, bool unslot = true)
    {
        UiAbility newUI = uiAbil.GetComponent<UiAbility>();
        newUI.setSlot(this);
        switch (mode)
        {
            case SlotMode.Equipment:
                if (unslot)
                {
                    itemTray.grabAbility(uiAbility);
                    string oldIndex = uiAbility.GetComponent<UiAbility>().inventoryIndex;
                    string newIndex = newUI.inventoryIndex;
                    gp.player.GetComponent<Inventory>().CmdEquipAbility(oldIndex, newIndex, invMode == InventoryMode.Drops);
                    FindObjectOfType<SoundManager>().playSound(SoundManager.SoundClip.Equip);
                }
                uiAbility = uiAbil;
                uiAbility.transform.SetParent(transform);
                uiAbility.GetComponent<UiAbility>().setDragger(null);
                uiAbility.transform.localPosition = Vector3.zero;
                break;
            case SlotMode.Trash:
                if (uiAbility)
                {
                    Destroy(uiAbility);
                }
                string index = uiAbil.GetComponent<UiAbility>().inventoryIndex;
                gp.player.GetComponent<Inventory>().CmdStageDelete(index);
                uiAbility = uiAbil;
                uiAbility.transform.SetParent(transform);
                uiAbility.transform.localPosition = Vector3.zero;
                break;
            case SlotMode.Store:

                string storeID = uiAbil.GetComponent<UiAbility>().inventoryIndex;
                gp.player.GetComponent<Inventory>().CmdSendStorage(storeID);
                Destroy(uiAbil);
                break;
        }

    }
    public void unslot()
    {
        switch (mode)
        {
            case SlotMode.Trash:
                gp.player.GetComponent<Inventory>().CmdUnstageDelete();
                uiAbility = null;
                break;
        }
    }

    public void clear()
    {
        if (uiAbility)
        {
            Destroy(uiAbility);
        }

    }
}
