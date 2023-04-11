using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiBuffIcon : MonoBehaviour
{
    public Image icon;

    public void setImage(Sprite sprite, Color c)
    {
        icon.sprite = sprite;
        icon.color = c;
    }
}
