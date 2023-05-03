using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Keybinds;

public class KeySetter : MonoBehaviour
{
    public Text label;
    public Button button;
    public Image key;

    KeyName keyname;

    public void setLabel(KeyName n, KeyCode k, Keybinds bind)
    {
        keyname = n;
        label.text = n.ToString();
        Sprite s = bind.keyImage(k);
        key.sprite = s;
        key.scaleToFit();
    }

    public void enableButton(bool e)
    {
        button.enabled = e;
    }

    public void setBinder(Keybinds bind)
    {
        button.onClick.AddListener(delegate { bind.callBind(keyname); });
    }
}
