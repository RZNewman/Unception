using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static GenerateAttack;
using static ItemList;
using static UnitControl;

public class UiEquipSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI label;

    public UiEquipmentDragger dragger;

    ItemSlot slotType;

    GameObject uiaCurrent;

    Keybinds keys;

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

    public void setItemSlot(ItemSlot slot)
    {
        keys = FindObjectOfType<Keybinds>(true);
        string keybind = keys.binding(toKeyName(slot)).ToString();
        label.text = slot.ToString() + ": " + keybind;
        label.gameObject.SetActive(true);
        slotType = slot;

    }

    public void slotObject(GameObject uiAbil, bool unslot = true)
    {
        UiAbility newUI = uiAbil.GetComponent<UiAbility>();
        if (newUI.blockFilled.slot != slotType)
        {
            dragger.storageGrab(uiAbil);
            return;
        }

        newUI.setSlot(this);
        if (unslot)
        {
            if (uiaCurrent)
            {
                dragger.storageGrab(uiaCurrent);
            }
            string newIndex = newUI.inventoryIndex;
            gp.player.GetComponent<Inventory>().CmdEquipAbility(newIndex);
            FindObjectOfType<SoundManager>().playSound(SoundManager.SoundClip.Equip);
        }
        uiaCurrent = uiAbil;
        uiaCurrent.transform.SetParent(transform);
        uiaCurrent.GetComponent<UiAbility>().setDragger(null);
        uiaCurrent.transform.localPosition = Vector3.zero;
        //switch (mode)
        //{
        //    case SlotMode.Equipment:

        //        break;
        //    case SlotMode.Trash:
        //        if (uiAbility)
        //        {
        //            Destroy(uiAbility);
        //        }
        //        string index = uiAbil.GetComponent<UiAbility>().inventoryIndex;
        //        gp.player.GetComponent<Inventory>().CmdStageDelete(index);
        //        uiAbility = uiAbil;
        //        uiAbility.transform.SetParent(transform);
        //        uiAbility.transform.localPosition = Vector3.zero;
        //        break;
        //    case SlotMode.Store:

        //        string storeID = uiAbil.GetComponent<UiAbility>().inventoryIndex;
        //        gp.player.GetComponent<Inventory>().CmdSendStorage(storeID);
        //        Destroy(uiAbil);
        //        break;
        //}

    }
    public void unslot()
    {
        //switch (mode)
        //{
        //    case SlotMode.Trash:
        //        gp.player.GetComponent<Inventory>().CmdUnstageDelete();
        //        uiAbility = null;
        //        break;
        //}
    }

    public void clear()
    {
        if (uiaCurrent)
        {
            Destroy(uiaCurrent);
        }

    }
}
