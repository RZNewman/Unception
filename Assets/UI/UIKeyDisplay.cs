using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Keybinds;

public class UIKeyDisplay : MonoBehaviour
{
    public KeyName key;
    // Start is called before the first frame update
    public void sync()
    {
        Keybinds keys = FindObjectOfType<Keybinds>(true);
        Image display = GetComponent<Image>();
        display.sprite = keys.keyImage(keys.binding(key));
        display.scaleToFit();
    }


}
