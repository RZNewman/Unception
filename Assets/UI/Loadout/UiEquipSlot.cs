using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ItemList;
using static UnitControl;

public class UiEquipSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI label;

    public UiEquipmentDragger dragger;
    public ItemList itemTray;
    public InventoryMode invMode;

    AttackKey attackKey;

    GameObject uiAbility;

    Keybinds keys;

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

    public void setAttackKey(AttackKey key)
    {
        keys = FindObjectOfType<Keybinds>(true);
        string keybind = keys.binding(toKeyName(key)).ToString();
        label.text = "A " + key.ToString() + ": " + keybind;
        label.gameObject.SetActive(true);
        attackKey = key;

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
                    if (invMode == InventoryMode.Storage)
                    {
                        itemTray.grabAbility(uiAbility);
                    }
                    string newIndex = newUI.inventoryIndex;
                    gp.player.GetComponent<Inventory>().CmdEquipAbility(attackKey, newIndex, invMode == InventoryMode.Drops);
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
