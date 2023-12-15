using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiTutorialSection : MonoBehaviour
{
    public TMP_Text text;
    public GameObject ImageLocation;
    public GameObject imagePre;


    public void setSection(PlayerInfo.TutorialSection section)
    {
        text.text = section.displayText;
        if(section.keybinds != null)
        {
            foreach (Keybinds.KeyName key in section.keybinds)
            {
                GameObject o = Instantiate(imagePre, ImageLocation.transform);
                UIKeyDisplay kd = o.GetComponent<UIKeyDisplay>();
                kd.key = key;
                kd.sync();

            }
        }
        
    }
}
